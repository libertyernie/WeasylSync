﻿namespace SourceWrappers

open PillowfortFs

type PillowfortPostWrapper(post: PillowfortPost, media: PillowfortMedia option) =
    interface IPostWrapper with
        member __.Title = post.title
        member __.HTMLDescription = post.content
        member __.Mature = post.nsfw
        member __.Adult = post.nsfw
        member __.Tags = post.tags
        member __.Timestamp = post.created_at.UtcDateTime
        member __.ViewURL = sprintf "https://pillowfort.io/posts/%d" post.id
        member __.ImageURL =
            match media with
            | Some m -> m.url
            | None -> post.avatar_url
        member __.ThumbnailURL =
            match media with
            | Some m -> m.url
            | None -> post.avatar_url

type PillowfortSourceWrapper(client: PillowfortClient) =
    inherit SourceWrapper<int>()

    let wrap p m = PillowfortPostWrapper (p, m) |> Swu.toPostWrapperInterface

    override __.Name = "Pillowfort"
    override __.SuggestedBatchSize = 20

    override __.Fetch cursor _ = async {
        let page = cursor |> Option.defaultValue 1
        let! response = client.AsyncGetPosts page
        
        let h = 20 * (page - 1) + (Seq.length response.posts) > response.total_count
        return {
            Posts = seq {
                for p in response.posts do
                    if Seq.isEmpty p.media then
                        yield wrap p None
                    else
                        for m in p.media do
                            yield wrap p (Some m)
            }
            Next = page + 1
            HasMore = h
        }
    }

    override __.Whoami = client.AsyncWhoami

    override __.GetUserIcon _ = client.AsyncGetAvatar