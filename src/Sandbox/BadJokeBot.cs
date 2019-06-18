using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Weavy.Core.Events;
using Weavy.Core.Models;
using Weavy.Core.Services;

namespace Weavy.Sandbox {

    /// <summary>
    /// A sample hook that reponds to posts, comments and messages with a bad joke.
    /// </summary>
    [Serializable]
    [Guid("B4795E3F-5007-475E-AB6F-124CCBFA52A7")]
    [Plugin(Icon="android", Name = "Bad joke bot", Description = "A bot that reponds to posts, comments and messages with a bad joke.")]
    public class BadJokeBot : Hook, IAsyncHook<AfterInsertPost>, IAsyncHook<AfterInsertComment>, IAsyncHook<AfterInsertMessage> {

        /// <summary>
        /// Gets or sets a keyword to listen for. Whenever a post or message contains this word. Our hook will reply with a random bad joke.
        /// </summary>
        [Required]
        [Display(Name = "Command", Description = "The command to listen for, e.g. /badjoke")]
        public string Keyword { get; set; }


        /// <summary>
        /// Reply to post with a random bad joke.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task HandleAsync(AfterInsertPost e) {
            if (Keyword != null) {
                if (e.Inserted.Text.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
                    var comment = CommentService.Insert(new Comment { Text = GetRandomJoke(), CreatedById = UserService.SystemId }, e.Inserted);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reply to comment with a random bad joke.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task HandleAsync(AfterInsertComment e) {
            if (Keyword != null) {
                if (e.Inserted.Text.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
                    var comment = CommentService.Insert(new Comment { Text = GetRandomJoke(), CreatedById = UserService.SystemId }, e.Inserted.Parent);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reply to message with a random bad joke.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task HandleAsync(AfterInsertMessage e) {
            if (Keyword != null) {
                if (e.Inserted.Text.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
                    var message = MessageService.Insert(new Message { Text = GetRandomJoke(), CreatedById = UserService.SystemId }, e.Inserted.Conversation);
                }
            }
            return Task.CompletedTask;
        }


        private static readonly Random _random = new Random();

        private static readonly string[] _jokes = {
            "I'M ON A SEAFOOD DIET. WHENEVER I SEE FOOD I EAT IT.",
            "WHAT DID THE FISH SAY WHEN IT RAN INTO A WALL? DAM.",
            "CAN FEBRUARY MARCH. NO. BUT APRIL MAY.",
            "WHAT DO YOU CALL A FISH WITH NO EYES? A FSH.",
            "YOU CAN TUNE A PIANO, BUT YOU CAN'T TUNA FISH! ...AMIRITE.",
            "WHAT'S RED AND SMELLS LIKE BLUE PAINT? RED PAINT",
            "I HAVE A FEAR OF SPEEDBUMPS. BUT I'M SLOWLY GETTING OVER IT.",
            "I CHANGED MY IPOD'S NAME TO TITANIC. IT'S SYNCING NOW.",
            "HOW MANY MEXICANS DOES IT TAKE TO CHANGE A LIGHT BULB? JUST JUAN.",
            "WHY CAN'T A BICYCLE STAND ON ITS OWN? BECAUSE IT'S TWO-TIRED.",
        };

        private string GetRandomJoke() {
            return _jokes[_random.Next(0, _jokes.Length)];
        }
    }
}
