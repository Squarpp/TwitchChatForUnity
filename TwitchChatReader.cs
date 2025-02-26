using System;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class TwitchChatReader : MonoBehaviour
{
    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public TextMeshPro textMeshProText;
    public string channelName; // Canal de Twitch a visualizar

    // En lugar de una única cadena, utilizamos una lista para almacenar cada línea
    private List<string> chatMessages = new List<string>();

    // Parámetros de control: cuando se supere maxMessages se conserva solo las últimas linesToKeep líneas.
    public int maxMessages = 100;
    public int linesToKeep = 20;

    void Start()
    {
        ConnectToTwitch();
    }

    void Update()
    {
        if (twitchClient != null && twitchClient.Connected)
        {
            if (twitchClient.Available > 0)
            {
                string message = reader.ReadLine();
                if (message != null)
                {
                    if (message.Contains("PRIVMSG"))
                    {
                        // Extraer el usuario (entre ':' y '!')
                        int userStart = message.IndexOf(':') + 1;
                        int userEnd = message.IndexOf('!');
                        string username = message.Substring(userStart, userEnd - userStart);

                        // Extraer el mensaje (después del segundo ':')
                        int splitPoint = message.IndexOf(":", 1);
                        string chatMessage = message.Substring(splitPoint + 1);
                        // Genera un color aleatorio
                        Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        string hexColor = ColorUtility.ToHtmlStringRGB(randomColor);

                        // Crea la línea de chat con el nombre en color aleatorio
                        string newLine = $"<color=#{hexColor}>{username}</color>: {chatMessage}";

                        chatMessages.Add(newLine);

                        // Si se supera el máximo de mensajes, se trunca la lista conservando solo las últimas líneas
                        if (chatMessages.Count > maxMessages)
                        {
                            chatMessages = chatMessages.GetRange(chatMessages.Count - linesToKeep, linesToKeep);
                        }

                        // Actualizar el texto del TextMeshPro uniendo las líneas con salto de línea
                        textMeshProText.text = string.Join("\n", chatMessages);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Reconectando...");
            ConnectToTwitch();
        }
    }

    void ConnectToTwitch()
    {
        try
        {
            twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream()) { AutoFlush = true };

            string username = "justinfan" + UnityEngine.Random.Range(1000, 9999);
            writer.WriteLine($"NICK {username}");
            writer.WriteLine($"JOIN #{channelName}");

            Debug.Log("Conectado al chat de Twitch!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error conectando a Twitch: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (twitchClient != null)
        {
            twitchClient.Close();
        }
    }
}
