﻿namespace FirstPlayable;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("First playable loaded.");

        FirstPlayableContext context = new();
        Scripts.DemoScene demoScene = new(context);
        foreach (object? _ in demoScene.Run())
        {
            Console.ReadLine();
        }
    }
}
