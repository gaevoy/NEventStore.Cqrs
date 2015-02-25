using Nancy;
using Nancy.ModelBinding;
using NEventStore.Cqrs;
using PetProject.Books.Projections;
using PetProject.Books.Shared.Commands;
using System;

namespace PetProject.Books.Host.Endpoints
{
    public class ApiModule : NancyModule
    {
        public ApiModule(IBookProjection books, ICommandBus commandBus)
        {
            Get["/api/Book.ListAll"] = _ => books.ListAll();
            Get["/api/Book.Load"] = _ => books.Load((Guid)Request.Query.id);
            Post["/api/Book.RegisterBook"] = _ =>
                {
                    var cmd = this.Bind<RegisterBook>();
                    cmd.Id = Guid.NewGuid();
                    commandBus.Publish(cmd);
                    return new { cmd.Id };
                };
            Post["/api/Book.CorrectBook"] = _ =>
                {
                    var cmd = this.Bind<CorrectBook>();
                    commandBus.Publish(cmd);
                    return null;
                };
            Post["/api/Book.DeleteBook"] = _ =>
                {
                    var cmd = this.Bind<DeleteBook>();
                    commandBus.Publish(cmd);
                    return null;
                };

            OnError += (ctx, ex) =>
                {
                    if (ex is AggregateException) ex = ((AggregateException)ex).Flatten().InnerExceptions[0];
                    var err = new
                        {
                            ErrorMessage = ex.Message,
                            ErrorType = ex.GetType().FullName,
                            StackTrace = ex.ToString()
                        };
                    return Response.AsJson(err, HttpStatusCode.InternalServerError);
                };
        }
    }
}
