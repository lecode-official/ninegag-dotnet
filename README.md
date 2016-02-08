# 9GAG.NET

![9GAG.NET Logo](https://github.com/lecode-official/ninegag-dotnet/blob/master/Documentation/Images/Banner.png "9GAG.NET Logo")

> *"9GAG has the best funny pics, GIFs, videos, memes, cute, wtf, geeky, cosplay photos on the web. 9GAG is your best source of fun."*
>
> &mdash; [9GAG.com](http://9gag.com/)

9GAG.NET is a simple, lightweight, and portable library for accessing 9GAG from managed languages. Since 9GAG does not seem to have an official API,
this library is essentially parsing the 9GAG website and extracts all the useful information. It abstracts away all the nasty HTML parsing and makes
it really simple to get all the posts from 9GAG. The library is also fully asynchronous and available for a lot of platforms.

## Using the Library

Right now the library is available as source code only, so you have to download and manually build the solution. The project was built using Visual
Studio 2015. Basically any version of Visual Studio 2015 will suffice, no extra plugins or tools are needed Just clone the Git repository, open the
solution in Visual Studio, and build it.

```batch
git pull https://github.com/lecode-official/ninegag-dotnet.git
```

## Sample

This is how it is to retrieve pages and posts from 9GAG:

```csharp
// Creates a new 9GAG client to access the 9GAG posts
using (NineGagClient nineGagClient = new NineGagClient())
{
    // Gets the first two pages of 9GAG
    IEnumerable<Section> sections = await nineGagClient.GetSectionsAsync();
    Section hotSection = sections.FirstOrDefault(section => section.Kind == SectionKind.Hot);
    List<Page> pages = new List<Page>();
    pages.Add(await nineGagClient.GetPostsAsync(hotSection));
    pages.Add(await nineGagClient.GetPostsAsync(hotSection, pages.Last()));

    // Prints all the retrieved pages
    foreach (Page page in pages)
    {
        System.Console.WriteLine();
        System.Console.WriteLine($"Page {pages.IndexOf(page) + 1}");
        System.Console.WriteLine();

        foreach (Post post in page.Posts)
            System.Console.WriteLine(post.Title);
    }
}
```

## Contributions

Currently I am not accepting any contributors, but if you want to help, I would greatly appreciate feedback and bug reports. To file a bug, please
use GitHub's issue system. Alternatively, you can clone the repository and send us a pull request.