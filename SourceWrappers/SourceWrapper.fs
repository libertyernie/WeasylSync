namespace SourceWrappers

open System

type IPostWrapper =
    abstract member Title: string with get
    abstract member HTMLDescription: string with get
    abstract member Mature: bool with get
    abstract member Adult: bool with get
    abstract member Tags: seq<string> with get
    abstract member Timestamp: DateTime with get
    abstract member ViewURL: string with get
    abstract member ImageURL: string with get
    abstract member ThumbnailURL: string with get

type IStatusUpdate =
    abstract member PotentiallySensitive: bool with get
    abstract member FullHTML: string with get
    abstract member HasPhoto: bool with get
    abstract member AdditionalLinks: seq<string> with get

type FetchResult<'cursor when 'cursor : struct> = {
    Posts: seq<IPostWrapper>
    Next: 'cursor
    HasMore: bool
}

[<AbstractClass>]
type SourceWrapper<'cursor when 'cursor : struct>() =
    abstract member Name: string with get
    
    abstract member Fetch: 'cursor option -> int -> Async<FetchResult<'cursor>>
    abstract member Whoami: unit -> Async<string>
    abstract member GetUserIcon: int -> Async<string>
    
    member this.StartAsync take = this.Fetch None take |> Async.StartAsTask
    member this.MoreAsync cursor take = this.Fetch cursor take |> Async.StartAsTask
    member this.WhoamiAsync () = this.Whoami () |> Async.StartAsTask
    member this.GetUserIconAsync size = this.GetUserIcon size |> Async.StartAsTask
