using System;
using System.Collections.Generic;
using System.Text.Json;

class Program
{
    static void Main()
    {
        string[] menuItems = { "Показать список текущих задач", "Показать список выполненных задач", "Добавить задачу", "Удалить задачу", "Отметить задачу выполненной", "Выход" };
        int selectedIndex = 0;
        bool isRunning = true;
        Console.CursorVisible = false;

        while (isRunning)
        {
            Console.Clear();
            ConsoleKey key;

            do
            {
                DisplayMenu(menuItems, selectedIndex);

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Length - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < menuItems.Length - 1) ? selectedIndex + 1 : 0;
                        break;
                }
            } while (key != ConsoleKey.Enter);

            isRunning = HandleMenuSelection(menuItems[selectedIndex]);
        }
    }

    static void DisplayMenu(string[] menuItems, int selectedIndex)
    {
        Console.SetCursorPosition(0, 0);

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine($"  {menuItems[i]}  ");
        }

        Console.ResetColor();
    }

    static bool HandleMenuSelection(string selectedItem)
    {
        Console.Clear();

        switch (selectedItem)
        {
            case "Показать список текущих задач":
                TodoTask.ShowTasks(false);
                break;
            case "Показать список выполненных задач":
                TodoTask.ShowTasks(true);
                break;
            case "Добавить задачу":
                TodoTask.AddTask();
                break;
            case "Удалить задачу":
                TodoTask.DeleteTask();
                break;
            case "Отметить задачу выполненной":
                TodoTask.CompleteTask();
                break;
            case "Выход":
                Console.WriteLine("Выход из программы...");
                return false;
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
        Console.ReadKey();
        return true;
    }
}

public class TodoTask
{
    private static readonly string filePath = @"D:\tasks.json";
    private static List<TodoTask> tasks = LoadFromFile();

    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }

    public TodoTask(string title, string? description = null)
    {
        Title = title;
        Description = description;
        IsCompleted = false;
    }

    public static void ShowTasks(bool completed)
    {
        var filteredTasks = tasks.Where(t => t.IsCompleted == completed).ToList();

        Console.WriteLine(completed ? "Выполненные задачи:" : "Текущие задачи:");

        if (filteredTasks.Count == 0)
        {
            Console.WriteLine("Нет задач.");
            return;
        }

        for (int i = 0; i < filteredTasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {filteredTasks[i].Title} ({filteredTasks[i].Description ?? "Без описания"})");
        }
    }

    public static void ShowTasks()
    {
        Console.WriteLine("Выполненные задачи помечены буквой В");
        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tasks[i].Title} ({tasks[i].Description ?? "Без описания"}) {(tasks[i].IsCompleted ? " - В" : "")}");
        }
    }

    public static void AddTask()
    {
        Console.Write("Введите заголовок задачи: ");
        string title = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Без названия";
        }
        Console.Write("Введите описание задачи или оставьте поле пустым: ");
        string description = Console.ReadLine()?.Trim();

        tasks.Add(new TodoTask(title, description));
        SaveToFile();
        Console.WriteLine("\nЗадача добавлена!");
    }

    public static void DeleteTask()
    {
        ShowTasks();
        Console.Write("Введите номер задачи для удаления: ");
        if (int.TryParse(Console.ReadLine(), out int taskNumber) && taskNumber > 0 && taskNumber <= tasks.Count)
        {
            tasks.RemoveAt(taskNumber - 1);
            SaveToFile();
            Console.WriteLine("Задача удалена!");
        }
        else
        {
            Console.Write("Некорректный номер задачи.");
        }
    }

    public static void CompleteTask()
    {
        var activeTasks = tasks.Where(t => !t.IsCompleted).ToList();

        if (activeTasks.Count == 0)
        {
            Console.WriteLine("\nНет незавершённых задач.");
            return;
        }

        ShowTasks(false);

        Console.Write("\nВведите номер задачи для завершения: ");
        if (int.TryParse(Console.ReadLine(), out int taskNumber) && taskNumber > 0 && taskNumber <= activeTasks.Count)
        {
            activeTasks[taskNumber - 1].IsCompleted = true;
            SaveToFile();
            Console.WriteLine("Задача отмечена как выполненная.");
        }
        else
        {
            Console.WriteLine("Некорректный номер задачи.");
        }
    }

    private static void SaveToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении файла: {ex.Message}");
        }
    }

    private static List<TodoTask> LoadFromFile()
    {
        if (!File.Exists(filePath))
            return new List<TodoTask>();

        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<TodoTask>>(json) ?? new List<TodoTask>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            return new List<TodoTask>();
        }
    }
}
