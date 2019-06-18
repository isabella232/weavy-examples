using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web.Api.Controllers;
using Weavy.Web.Api.Models;

namespace Weavy.Sandbox {

    /// <summary>
    /// JSON API endpoints that we want to expose in the sandboxes and demos.
    /// </summary>
    [RoutePrefix("api")]
    public class ApiController : WeavyApiController {

        /// <summary>
        /// Add member(s) to a space.
        /// </summary>
        /// <param name="id">Id of the space for which to add members.</param>
        /// <param name="members">The ids of the users to add</param>
        /// <returns>The inserted members.</returns>
        [HttpPost]
        [Route("spaces/{id:int}/members")]
        public IEnumerable<Member> AddMembers(int id, MembersIn members) {
            var inserted = new List<Member>();
            foreach (var userid in members.Members) {
                var user = UserService.Get(userid);
                if (user != null) {
                    inserted.Add(SpaceService.AddMember(id, user.Id));
                }
            }
            return inserted;
        }

        /// <summary>
        /// Get apps in the specified space.
        /// </summary>
        /// <param name="id">Id of the space for which to get apps.</param>
        /// <returns>A list of apps.</returns>
        [HttpGet]
        [Route("spaces/{id:int}/apps")]
        public IEnumerable<App> GetApps(int id) {
            return AppService.GetApps(id);
        }

        /// <summary>
        /// Creates a new post in the specified app.
        /// </summary>
        /// <param name="id">The app id.</param>
        /// <param name="post">The post to insert.</param>
        /// <returns>The inserted post.</returns>
        [HttpPost]
        [Route("apps/{id:int}/posts")]
        public Post InsertPost(int id, PostIn post) {
            var app = AppService.Get(id);
            var model = new Post();
            model.Text = post.Text;
            return PostService.Insert(model, app, blobs: post.Blobs, embeds: post.Embeds, options: post.Options?.Select(s => new PollOption { Text = s }));
        }


        /// <summary>
        /// Performs a search for spaces.
        /// </summary>
        /// <param name="query">The <see cref="SpaceQuery"/> object that constitutes the search.</param>
        /// <returns>A list of matching spaces.</returns>
        [HttpGet]
        [Route("spaces")]
        public ScrollableList<Space> SearchSpaces(SpaceQuery query) {
            var result = SpaceService.Search(query);
            return new ScrollableList<Space>(result, query.Top, query.Skip, result.TotalCount, Request.RequestUri);
        }

        /// <summary>
        /// Searches for users.
        /// </summary>
        /// <param name="query">The <see cref="UserQuery"/> that constitutes the search.</param>
        /// <returns>A list of matching users.</returns>
        [HttpGet]
        [Route("users")]
        public ScrollableList<User> SearchUsers(UserQuery query) {
            var result = UserService.Search(query);
            return new ScrollableList<User>(result, result.Top, result.Skip, result.TotalCount, Request.RequestUri);
        }
    }
}
