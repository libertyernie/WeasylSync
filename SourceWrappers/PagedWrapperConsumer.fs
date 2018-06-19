﻿namespace SourceWrappers

open System.Threading.Tasks

/// Consumes IPagedSourceWrapper, one page at a time. Some wrappers might not support a constant page size and need to be wrapped in a CachedSourceWrapper first.
type IPagedWrapperConsumer =
    abstract member Name: string with get
    abstract member HasMore: bool with get
    abstract member SuggestedBatchSize: int with get
    abstract member NextAsync: unit -> Task<seq<IPostWrapper>>
    abstract member PrevAsync: unit -> Task<seq<IPostWrapper>>
    abstract member FirstAsync: unit -> Task<seq<IPostWrapper>>
    abstract member FetchAllAsync: int -> Task<seq<IPostWrapper>>
    abstract member WhoamiAsync: unit -> Task<string>
    abstract member GetUserIconAsync: int -> Task<string>

type internal PagedWrapperCursor<'a> = {
    cursor: 'a
    last: PagedWrapperCursor<'a> option
}

/// Consumes IPagedSourceWrapper, one page at a time. Some wrappers might not support a constant page size and need to be wrapped in a CachedSourceWrapper first.
type PagedWrapperConsumer<'a when 'a : struct>(wrapper: ISourceWrapper<'a>, page_size: int) =
    let mutable next_cursor: PagedWrapperCursor<'a> option = None
    let mutable has_more = true

    let back cursor =
        match cursor with
        | Some c -> c.last
        | None -> None

    let next = async {
        let! r =
            match next_cursor with
            | Some c -> wrapper.MoreAsync c.cursor page_size |> Async.AwaitTask
            | None -> wrapper.StartAsync page_size |> Async.AwaitTask

        if Seq.length r.Posts > page_size then failwith "This wrapper does not support custom page sizes."

        let n = {
            cursor = r.Next
            last = next_cursor
        }
        next_cursor <- Some n
        has_more <- r.HasMore
        return r.Posts
    }

    let prev = async {
        next_cursor <- next_cursor |> back |> back
        return! next
    }

    let first = async {
        next_cursor <- None
        return! next
    }

    interface IPagedWrapperConsumer with
        member this.Name = wrapper.Name
        member this.HasMore = has_more
        member this.SuggestedBatchSize = wrapper.SuggestedBatchSize
        member this.NextAsync() = next |> Async.StartAsTask
        member this.PrevAsync() = prev |> Async.StartAsTask
        member this.FirstAsync() = first |> Async.StartAsTask
        member this.FetchAllAsync limit = wrapper.FetchAllAsync limit
        member this.WhoamiAsync() = wrapper.WhoamiAsync()
        member this.GetUserIconAsync size = wrapper.GetUserIconAsync size