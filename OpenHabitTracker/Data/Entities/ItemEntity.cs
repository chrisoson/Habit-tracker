﻿namespace OpenHabitTracker.Data.Entities;

public class ItemEntity
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? DoneAt { get; set; }
}
