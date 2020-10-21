using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.Services.Sys
{
    public class MessageBuilder
    {
        public static (EmbedBuilder,string) EmbedUserBuilder(string text)
        {
            var emb = new EmbedBuilder();
            Root EB;
            try
            {
                EB = JsonConvert.DeserializeObject<Root>(text);
            }
            catch (Exception)
            {
                return (emb, "ERROR");
            }

            if (EB.title != null) emb.WithTitle(EB.title);
            if (EB.description != null) emb.WithDescription(EB.description);
            if (EB.author != null) emb.WithAuthor(EB.author.name, EB.author.icon_url, EB.author.url);
            if (EB.color != 0) emb.WithColor(new Color(EB.color));
            if (EB.footer != null) emb.WithFooter(EB.footer.text, EB.footer.icon_url);
            if (EB.thumbnail != null) emb.WithThumbnailUrl(EB.thumbnail);
            if(EB.image != null) emb.WithImageUrl(EB.image);
            if (EB.fields != null)
            {
                foreach (var field in EB.fields)
                {
                    if (field.value != null && field.name != null && field.value != "" && field.name != "")
                        emb.AddField(field.name, field.value, field.inline);
                }
            }
            return (emb,EB.plainText);
        } // EMBED СООБЩЕНИЕ

        private class Root
        {
            public string plainText { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public Author author { get; set; }
            public uint color { get; set; }
            public Footer footer { get; set; }
            public string thumbnail { get; set; }
            public string image { get; set; }
            public List<Field> fields { get; set; }
        } // EMBED BD
        private class Author
        {
            public string name { get; set; }
            public string url { get; set; }
            public string icon_url { get; set; }
        }// EMBED BD
        private class Footer
        {
            public string text { get; set; }
            public string icon_url { get; set; }
        }// EMBED BD
        private class Field
        {
            public string name { get; set; }
            public string value { get; set; }
            public bool inline { get; set; }
        }// EMBED BD
    }
}
