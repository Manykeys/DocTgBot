using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace TelegramWordBot;

static class DocumentManager
{
    public static Dictionary<long, List<string>> UserDocuments { get; private set; } = new();

    public static string CreateNewDocument(long chatId)
    {
        var documentName = $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
        var filePath = Path.Combine(Path.GetTempPath(), documentName);

        using (var wordDocument = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Word.Document();
            var docBody = new Word.Body();

            var table = new Word.Table();
            var headerRow = new Word.TableRow();
            headerRow.Append(
                new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text("Колонка 1")))),
                new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text("Колонка 2")))),
                new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text("Колонка 3"))))
            );
            table.Append(headerRow);

            docBody.Append(table);
            mainPart.Document.Append(docBody);
            mainPart.Document.Save();
        }

        if (!UserDocuments.ContainsKey(chatId))
        {
            UserDocuments[chatId] = new List<string>();
        }
        UserDocuments[chatId].Add(documentName);

        return filePath;
    }

    private static void EditDocument(long chatId, string documentName, string column1Text, string column2Text, string column3Text)
    {
        var filePath = Path.Combine(Path.GetTempPath(), documentName);

        using (var wordDocument = WordprocessingDocument.Open(filePath, true))
        {
            var docBody = wordDocument.MainDocumentPart.Document.Body;

            var table = docBody.Elements<Word.Table>().FirstOrDefault();
            if (table != null)
            {
                var dataRow = new Word.TableRow();
                dataRow.Append(
                    new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(column1Text)))),
                    new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(column2Text)))),
                    new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(column3Text))))
                );
                table.Append(dataRow);
            }

            wordDocument.MainDocumentPart.Document.Save();
        }
    }

    private static async Task SendDocument(ITelegramBotClient botClient, long chatId, string documentName, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(Path.GetTempPath(), documentName);

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await botClient.SendDocumentAsync(
                chatId: chatId,
                document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream, documentName),
                caption: "Ваш документ",
                cancellationToken: cancellationToken
            );
        }
    }

    public static async Task ProcessDocumentCommands(ITelegramBotClient botClient, long chatId, string messageText, CancellationToken cancellationToken)
    {
        if (messageText.StartsWith("Документ: "))
        {
            var documentName = messageText.Replace("Документ: ", "");
            await MenuManager.EditDocumentMenu(botClient, chatId, documentName, cancellationToken);
        }
        else if (messageText.StartsWith("Скачать: "))
        {
            var documentName = messageText.Replace("Скачать: ", "");
            await SendDocument(botClient, chatId, documentName, cancellationToken);
        }
        else if (messageText.StartsWith("Изменить документ: "))
        {
            var parts = messageText.Replace("Изменить документ: ", "").Split('|');
            if (parts.Length == 4)
            {
                var documentName = parts[0].Trim();
                var column1Text = parts[1].Trim();
                var column2Text = parts[2].Trim();
                var column3Text = parts[3].Trim();
                EditDocument(chatId, documentName, column1Text, column2Text, column3Text);
                await botClient.SendTextMessageAsync(chatId, "Документ изменён!", cancellationToken: cancellationToken);
            }
        }
    }
}