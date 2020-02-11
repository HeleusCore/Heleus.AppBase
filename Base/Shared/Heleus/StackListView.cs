using System;
using System.Collections.Generic;

namespace Heleus.Apps.Shared
{
    public abstract class StackListView<RowType, ItemType> where RowType : StackRow
    {
        protected readonly StackRow _header;
        protected readonly StackPage _page;

        public IReadOnlyList<RowType> Rows => new List<RowType>(_rows);
        readonly List<RowType> _rows = new List<RowType>();

        protected StackListView(StackPage page, StackRow header)
        {
            _page = page;
            _header = header;
        }

        protected abstract RowType AddRow(StackPage page, ItemType item);
        protected abstract void UpdateRow(RowType row, ItemType newItem);

        protected virtual void UpdateDone()
        {

        }

        public void Remove(ItemType item)
        {
            if (item.IsNullOrDefault())
                return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (item.Equals(_rows[i]))
                {
                    _rows.RemoveAt(i);
                    return;
                }
            }
        }

        public void Update(List<ItemType> items)
        {
            _page.AddIndex = _header;
            _page.AddIndexBefore = false;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (i < _rows.Count && _rows[i].Tag is ItemType)
                {
                    var row = _rows[i];
                    UpdateRow(row, item);
                    row.Tag = item;
                    _page.AddIndex = row;
                }
                else
                {
                    var row = AddRow(_page, item);
                    if (row != null)
                    {
                        row.Tag = item;
                        _page.AddIndex = row;
                        _rows.Add(row);
                    }
                }
            }

            while(_rows.Count > items.Count)
            {
                var i = _rows.Count - 1;

                _page.RemoveView(_rows[i]);
                _rows.RemoveAt(i);
            }

            _page.AddIndex = null;
            UpdateDone();
        }
    }
}
