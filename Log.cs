using System;
using System.Collections.Generic;
using System.IO;

internal class Log
{
    private string logDirectory;
    private string logName;
    private string logFormat;

    public Log(string logDirectory, string logName, string logFormat)
    {
        this.logDirectory = logDirectory;
        this.logName = logName;
        this.logFormat = logFormat;
    }

    public void UniqueWords<T>(List<T> words)
    {
        // Utilizar HashSet para eliminar duplicados automáticamente
        HashSet<T> uniqueWords = new HashSet<T>(words);

        // Construir el texto con las palabras únicas
        string text = string.Join(Environment.NewLine, uniqueWords);

        // Registrar las palabras únicas usando WeeklyLog
        WritteWeeklyLog(text, true);
    }

    public void WritteWeeklyLog(string text, bool noDuplicates = false)
    {
        // Crear el nombre del archivo de log basado en la fecha del día actual
        string logFilePath = Path.Combine(logDirectory, logName + "_" + DateTime.Now.ToString("yyyyMMdd") + $".{logFormat}");

        // Verificar si el directorio existe, y si no, crearlo
        if (!Directory.Exists(logDirectory))
        {
            try
            {
                Directory.CreateDirectory(logDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creando directorio: " + e.ToString());
                return;
            }
        }

        // Si se deben evitar duplicados
        if (noDuplicates)
        {
            // Crear un HashSet para almacenar las entradas únicas
            HashSet<string> logEntries = new HashSet<string>();

            // Si el archivo ya existe, cargar sus entradas en el HashSet
            if (File.Exists(logFilePath))
            {
                try
                {
                    string[] existingEntries = File.ReadAllLines(logFilePath);
                    foreach (var entry in existingEntries)
                    {
                        logEntries.Add(entry); // Añadir las entradas existentes
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error leyendo el archivo de log: " + e.ToString());
                    return; // Salir si hay error
                }
            }

            // Dividir el texto recibido en nuevas entradas y agregarlas al HashSet
            string[] newEntries = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in newEntries)
            {
                logEntries.Add($"{DateTime.Now:HH:mm:ss} - {entry}");  // Añadir con la hora actual
            }

            // Escribir las entradas únicas (existentes + nuevas) en el archivo
            try
            {
                File.WriteAllText(logFilePath, string.Join(Environment.NewLine, logEntries) + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error escribiendo en el archivo de log: " + e.ToString());
            }
        }
        else
        {
            // Si no se deben evitar duplicados, simplemente añadir el texto al archivo sin verificar
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now:HH:mm:ss} - {text}" + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error escribiendo en el archivo de log: " + e.ToString());
            }
        }
    }


    //Genera un log por instancia de Log
    public void WritteInLogDebug(string text)
    {
        string logFilePath = Path.Combine(logDirectory, logName + "_" + DateTime.Now.ToString("ddMMyy_HHmmss") + $".{logFormat}");

        if (!Directory.Exists(logDirectory))
        {
            // Crear el directorio si no existe
            try
            {
                Directory.CreateDirectory(logDirectory);
            }
            catch (Exception e) { Console.Write(e.ToString()); }
        }
        try
        {
            // Añadir el mensaje al log
            File.AppendAllText(logFilePath, text + Environment.NewLine);
        }
        catch (Exception e) { Console.Write(e.ToString()); }
    }
}
