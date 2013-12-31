﻿using Ididit.Data.Models;

namespace Ididit.Data;

public class UserData
{
    public List<HabitModel>? Habits { get; set; }
    public List<NoteModel>? Notes { get; set; }
    public List<TaskModel>? Tasks { get; set; }
    public List<TrashModel>? Models { get; set; }
}
