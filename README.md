# 9GAG.NET

> **WARNING!** Currently the 9GAG.NET library is being completely rewritten due to changes in the way the 9GAG website works. This means that, for now, the library is broken.

![9GAG.NET Logo](https://github.com/lecode-official/ninegag-dotnet/blob/master/documentation/images/logo-banner.png "9GAG.NET Logo")

> *"9GAG has the best funny pics, GIFs, videos, memes, cute, wtf, geeky, cosplay photos on the web. 9GAG is your best source of fun."*
>
> &mdash; [9GAG.com](http://9gag.com/)

9GAG.NET is a simple, lightweight, and portable library for accessing 9GAG from managed languages. Since 9GAG does not seem to have an official API, this library is essentially parsing the 9GAG website and extracts all the useful information. It abstracts away all the nasty HTML parsing and makes it really simple to get all the posts from 9GAG. The library is also fully asynchronous and available for a lot of platforms.

## Using the Project

The project is available on [NuGet](https://www.nuget.org/packages/NineGag/).

```batch
PM> Install-Package NineGag
```

## Samples

The central class in 9GAG.NET is `NineGagClient`, it implements the `IDisposable` interface, so always make sure, that you are using the `using`-statement or call `Dispose` by hand.

```csharp
using (NineGagClient nineGagClient = new NineGagClient())
{
}
```

This is is how you retrieve posts form 9GAG. The result of retrieving posts is always a page of 10 posts. Alls calls to 9GAG may throw exceptions, so make sure to always wrap you calls in a `try`-`catch`-statement:

```csharp
try
{
    Page page = await nineGagClient.GetPostsAsync();
}
catch (NineGagException) { }
```

You can retrieve the next page by passing the retrieved page as a parameter to `GetPostsAsync`:

```csharp
Page nextPage = await nineGagClient.GetPostsAsync(page);
```

You can also retrieve posts by actuality:

```csharp
Page page = await nineGagClient.GetPostsAsync(PostActuality.Trending);
```

Or by section and sub-section:

```csharp
IEnumerable<Section> sections = await nineGagClient.GetSectionsAsync();
Page page = await nineGagClient.GetPostsAsync(sections.First());
Page subSectionPage = await nineGagClient.GetPostsAsync(sections.First(), SubSection.Fresh);
```

You can also sign in to 9GAG and upvote/downvote posts:

```csharp
if (await nineGagClient.SignInAsync("UserName", "Password"))
{
    Page page = await nineGagClient.GetPostsAsync(PostActuality.Trending);
    await page.Posts.First().UpvoteAsync();
    await page.Posts.Last().DownvoteAsync();
    await nineGagClient.SignOutAsync();
}
```

When you are signed in, then you can retrieve some information about the user:

```csharp
if (await nineGagClient.SignInAsync("UserName", "Password"))
{
    User user = await nineGagClient.GetCurrentUserAsync();
    await nineGagClient.SignOutAsync();
}
```

All methods in the `NineGagClient` are asynchronous an can be cancelled:

```csharp
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
Page page = await nineGagClient.GetPostsAsync(PostActuality.Hot, cancellationTokenSource.Token);
```

## Contributions

Currently I am not accepting any contributors, but if you want to help, I would greatly appreciate feedback and bug reports. To file a bug, please use GitHub's issue system. Alternatively, you can clone the repository and send me a pull request.
