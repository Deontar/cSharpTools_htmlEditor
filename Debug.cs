using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


internal class Debug
{
    private string debugFilePath;
    private int wordsIdentifiedCount;
    private int wordsTranslatedCount;
    private int wordsNotTranslatedCount;
    private List<string> translatedWords;
    private List<string> notTranslatedWords;

    /// <summary>
    /// Inicializa una nueva instancia de la clase logDebug.
    /// </summary>
    /// <param name="debugFilePath">Ruta del archivo de depuración donde se escribirán los registros.</param>
    /// <param name="wordsIdentifiedCount">Número total de palabras identificadas durante la traducción.</param>
    /// <param name="wordsTranslatedCount">Número total de palabras traducidas con éxito.</param>
    /// <param name="wordsNotTranslatedCount">Número total de palabras que no fueron traducidas.</param>
    /// <param name="translatedWords">Lista de palabras traducidas.</param>
    /// <param name="notTranslatedWords">Lista de palabras no traducidas.</param>
    public Debug(string debugFilePath, int wordsIdentifiedCount, int wordsTranslatedCount, int wordsNotTranslatedCount, List<string> translatedWords, List<string> notTranslatedWords)
    {
        this.debugFilePath = debugFilePath;
        this.wordsIdentifiedCount = wordsIdentifiedCount;
        this.wordsTranslatedCount = wordsTranslatedCount;
        this.wordsNotTranslatedCount = wordsNotTranslatedCount;
        this.translatedWords = translatedWords;
        this.notTranslatedWords = notTranslatedWords;


    }
    /// <summary>
        /// Genera un resumen de la traducción, incluyendo la cantidad de palabras identificadas, traducidas y no traducidas.
        /// Escribe el resumen en el archivo de depuración.
        /// </summary>
    public void GenerateSummary()
    {
        try
        {
            Log("\r\n--- Resumen de Traducción ---");
            Log($"Palabras identificadas: {wordsIdentifiedCount}");
            Log($"Palabras traducidas: {wordsTranslatedCount}");
            Log($"Palabras no traducidas: {wordsNotTranslatedCount}");

            Log("\r\n--- Palabras traducidas ---");
            Log("\r[palabra tratada para comparación]\t\t\t[palabra en formato original]");
            foreach (var translated in translatedWords)
            {
                Log($"{translated.ToLower().Trim()}\t\t\t{translated}");
            }

            Log("\r\n--- Palabras no traducidas ---");
            Log("\r[palabra tratada para comparación]\t\t\t[palabra en formato original]");
            foreach (var notTranslated in notTranslatedWords)
            {
                Log($"{notTranslated.ToLower().Trim()}\t\t\t{notTranslated}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en GenerateSummary: {e.Message}");
            throw;
        }
    }
    /// <summary>
    /// Registra un mensaje en el archivo de depuración.
    /// </summary>
    /// <param name="message">El mensaje que se escribirá en el archivo de depuración.</param>
    public void Log(string message)
    {
        string directoryPath = Path.GetDirectoryName(debugFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        try
        {
            File.AppendAllText(debugFilePath, message + Environment.NewLine);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al escribir en el archivo de depuración: {e.Message}");
        }
    }
}