using System;
using System.Text;
using System.IO;


public class Class1
{

    static void dataCSV(int[] encoderVals, int[] colourVals)
    {
        var csv = new StringBuilder;
        string filepath = @"c:\temp\mytest.txt";

        for (int i = 0; i < 4; i++){
            var first = encoderVals[i].ToString();
            var second = colourVals[i].ToString();

            var newLine = string.Format("{0},{1},{2}", first, second, Environment.NewLine);
            csv.Append(newLine);
        }
        File.WriteAllText(filepath, csv.ToString());
    }
    public Class1()
	{
        int[] encoders = new int[5] { 1, 2, 3, 5, 3 };
        int[] colours = new int[5] { 0, 9, 8, 7, 6 };
        Console.WriteLine(encoders, colours);
        Console.WriteLine("got to the function");
        dataCSV(encoders, colours);
	}
}
