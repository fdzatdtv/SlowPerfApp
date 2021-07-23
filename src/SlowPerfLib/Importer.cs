using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SlowPerfLib
{
    /// <summary>
    /// Imports data from csv file.
    /// </summary>
    /// <remarks>
    /// Imports data from csv file and returns a <see cref="DataSet"/>.
    /// Csv file must have headers in first line.
    /// </remarks>
    /// <typeparam name="T">Any type having properties attributed with <see cref="TableColumnAttribute"/> makes sense.</typeparam>
    public class CsvImporter<T>
    {
        private DataSet _dataset = new DataSet();

        public DataSet Data
        {
            get => _dataset;
            set
            {
                _dataset = value;
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CsvImporter()
        {
            InitData();
        }

        private void InitData()
        {
            _dataset.Tables.Add(new DataTable("RecordsTable"));
        }

        /// <summary>
        /// Notifies when <see cref="Data"/> has changed
        /// </summary>
        public event EventHandler DataChanged;

        /// <summary>
        /// Notifies about progress during <see cref="Import"/>
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Imports data from csv file.
        /// </summary>
        /// <remarks>
        /// Headers must be in first line and be equal to properties attributed with <see cref="TableColumnAttribute"/>.
        /// </remarks>
        /// <param name="filepath">Path to csv file to be imported</param>
        /// <returns>DataSet with one table containing data of csv file matching properties of type <typeparam name="T"/></returns>
        public DataSet Import(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                DataSet dataset = Data ?? (Data = new DataSet());
                ClearData();
                dataset.Tables.Add(new DataTable("RecordsTable"));
                return Data;
            }
            else
            {
                var reader = File.OpenText(filepath);
                string content = reader.ReadToEnd();
                var lines = content.Split('\n').Where(line => !string.IsNullOrEmpty(line)).ToArray();
                var dataObjects = new DataObjects<T>(lines.Length);
                for (int rowIndex = 1; rowIndex < lines.Length; rowIndex++)
                    dataObjects.Add(new DataObject<T>(rowIndex, content));
                return Data = FillDataSetFrom(dataObjects);
            }
        }

        private DataSet FillDataSetFrom(DataObjects<T> dataObjects)
        {
            DataSet dataset = Data ?? (Data = new DataSet());

            if (!dataset.Tables.Contains("RecordsTable"))
            {
                dataset.Tables.Add(new DataTable("RecordsTable"));

                CreateTableColumnsFrom(dataObjects);
            }
            else
            {
                ClearData();
                dataset.Tables.Add(new DataTable("RecordsTable"));
                CreateTableColumnsFrom(dataObjects);
            }

            InsertDataObjects(dataObjects, dataset);

            return dataset;
        }

        private void InsertDataObjects(DataObjects<T> dataObjects, DataSet dataset)
        {
            int index = 0;
            int total = dataObjects.Count;
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(total, index, ProgressAction.Start));
            foreach (var dataObject in dataObjects.Get())
            {
                InsertDataObject(dataset, dataObject);
                index++;
                ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(total, index, ProgressAction.InProgress));
            }
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(total, index, ProgressAction.End));
        }

        private void ClearData()
        {
            var rows = Data.Tables["RecordsTable"].Rows;
            for (int i=0; i<rows.Count; i++)
                rows.RemoveAt(0);
            Data.Tables.Remove("RecordsTable");
        }

        private void CreateTableColumnsFrom(DataObjects<T> dataObjects)
        {
            var table = Data.Tables["RecordsTable"];

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var tableColumnAttribute = propertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (tableColumnAttribute != null)
                    table.Columns.Add(tableColumnAttribute.ColName, propertyInfo.PropertyType);
            }
        }

        private void InsertDataObject(DataSet dataset, DataObject<T> dataObject)
        {
            var table = dataset.Tables["RecordsTable"];

            var datarow = table.NewRow();

            var columnNames = dataObject.GetColumnNames();

            foreach (var columnName in columnNames)
                datarow[columnName] = dataObject.GetValue(columnName);

            table.Rows.Add(datarow);
        }

    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public int TotalRows { get; }
        public int ProcessedRows { get; }
        public ProgressAction Action { get; }

        public ProgressChangedEventArgs(int totalRows, int processedRows, ProgressAction action)
        {
            TotalRows = totalRows;
            ProcessedRows = processedRows;
            Action = action;
        }
    }

    public enum ProgressAction
    {
        Start,
        InProgress,
        End,
    }

    /// <summary>
    /// Container for <see cref="DataObject{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DataObjects<T>
    {
        private List<DataObject<T>> _dataObjects = new List<DataObject<T>>();

        public DataObjects(int capacity)
        {
            _dataObjects = new List<DataObject<T>>(capacity);
        }

        public int Count => _dataObjects.Count;

        public void Add(DataObject<T> dataObject)
        {
            foreach (var entry in _dataObjects)
            {
                if (dataObject.RowIndex == entry.RowIndex)
                    return;
            }
            _dataObjects.Add(dataObject);
        }

        public DataObject<T>[] Get()
        {
            return _dataObjects.ToArray();
        }
    }

    /// <summary>
    /// Csv value reader for a single line
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DataObject<T>
    {
        private readonly int _rowIndex;
        private readonly string _source;

        public int RowIndex => _rowIndex;

        public DataObject(int rowIndex, string source)
        {
            _rowIndex = rowIndex;
            _source = source;
        }

        public string[] GetColumnNames()
        {
            var columnNames = new List<string>();
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var tableColumnAttribute = propertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (tableColumnAttribute != null)
                    columnNames.Add(tableColumnAttribute.ColName);
            }

            return columnNames.ToArray();
        }

        public object GetValue(string columnName)
        {
            PropertyInfo pi = GetPropertyInfo(columnName);
            var data = GetRecord();
            return pi.GetValue(data);
        }

        private PropertyInfo GetPropertyInfo(string columnName)
        {
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var tableColumnAttribute = propertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (tableColumnAttribute != null && tableColumnAttribute.ColName == columnName)
                    return propertyInfo;
            }

            return null;
        }

        private object GetRecord()
        {
            var record = Activator.CreateInstance(typeof(T));
            FillRecord(record);
            return record;
        }

        private void FillRecord(object record)
        {
            var lines = _source.Split('\n');
            var headers = lines[0].Split(';');
            int headerIndex = 0;
            foreach (var header in headers)
            {
                PropertyInfo pd = record.GetType().GetProperty(header);
                string dataline = lines[_rowIndex];
                var csvValues = dataline.Split(';');
                var csvValue = csvValues[headerIndex++];

                if (pd == null)
                    continue;

                try
                {
                    if (pd.PropertyType == typeof(DateTime))
                        pd.SetValue(record, DateTime.Parse(csvValue));
                    else if (pd.PropertyType == typeof(TimeSpan))
                        pd.SetValue(record, TimeSpan.Parse(csvValue));
                    else if (pd.PropertyType == typeof(decimal))
                        pd.SetValue(record, decimal.Parse(csvValue));
                    else if (pd.PropertyType == typeof(bool))
                        pd.SetValue(record, bool.Parse(csvValue));
                    else if (pd.PropertyType == typeof(float))
                        pd.SetValue(record, float.Parse(csvValue));
                    else if (pd.PropertyType == typeof(float))
                        pd.SetValue(record, float.Parse(csvValue));
                    else if (pd.PropertyType == typeof(string))
                        pd.SetValue(record, csvValue);
                    else if (pd.PropertyType == typeof(int))
                        pd.SetValue(record, int.Parse(csvValue));
                    else if (pd.PropertyType == typeof(uint))
                        pd.SetValue(record, uint.Parse(csvValue));
                    else if (pd.PropertyType == typeof(long))
                        pd.SetValue(record, long.Parse(csvValue));
                    else if (pd.PropertyType == typeof(short))
                        pd.SetValue(record, short.Parse(csvValue));
                    else if (pd.PropertyType == typeof(ushort))
                        pd.SetValue(record, ushort.Parse(csvValue));
                    else if (pd.PropertyType == typeof(long))
                        pd.SetValue(record, long.Parse(csvValue));
                    else if (pd.PropertyType == typeof(double))
                        pd.SetValue(record, double.Parse(csvValue));
                    else if (pd.PropertyType.IsEnum)
                        pd.SetValue(record, Enum.Parse(pd.PropertyType, csvValue));
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public class TableColumnAttribute : Attribute
    {
        public string ColName { get; set; }

        public TableColumnAttribute(string columnName)
        {
            ColName = columnName;
        }
    }
}
