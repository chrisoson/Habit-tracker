﻿using Ididit.Data;
using Ididit.Data.Entities;
using Ididit.Data.Models;

namespace Ididit.Services;

public class ItemService(IDataAccess dataAccess)
{
    private readonly IDataAccess _dataAccess = dataAccess;

    public ItemModel? SelectedItem { get; set; }

    public ItemModel? NewItem { get; set; }

    public ItemModel? EditItem { get; set; }

    public async Task Initialize(ItemsModel? items)
    {
        if (items is not null)
        {
            if (items.Items is null)
            {
                IReadOnlyList<ItemEntity> itms = await _dataAccess.GetItems(items.Id);

                items.Items = itms.Select(i => new ItemModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    IsDone = i.IsDone
                }).ToList();
            }
        }

        NewItem ??= new();
    }

    public async Task AddItem(ItemsModel? items)
    {
        if (items is null || NewItem is null)
            return;

        if (items.Items is null)
            items.Items = new();

        items.Items.Add(NewItem);

        ItemEntity item = new ItemEntity
        {
            ParentId = items.Id,
            Title = NewItem.Title,
            IsDone = false
        };

        await _dataAccess.AddItem(item);

        NewItem.Id = item.Id;

        NewItem = new();
    }

    public async Task UpdateItem()
    {
        if (EditItem is null)
            return;

        if (await _dataAccess.GetItem(EditItem.Id) is ItemEntity item)
        {
            item.Title = EditItem.Title;
            item.IsDone = EditItem.IsDone;

            await _dataAccess.UpdateItem(item);
        }
    }

    public async Task SetIsDone(ItemModel item, bool done)
    {
        item.IsDone = done;

        if (await _dataAccess.GetItem(item.Id) is ItemEntity itemEntity)
        {
            itemEntity.IsDone = done;

            await _dataAccess.UpdateItem(itemEntity);
        }
    }

    public async Task DeleteItem(ItemsModel? items, ItemModel item)
    {
        items?.Items?.Remove(item);

        await _dataAccess.RemoveItem(item.Id);
    }
}