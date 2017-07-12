# Papyrus UWP
An ePub parser for the Universal Windows Platform (UWP). Also has some basic controls to demonstrate displaying ebooks.

## Usage
This details basic usage of the core Papyrus parser, the Papyrus HTML parser and the Papyrus UI controls.

### Papyrus - Parsing an ePub file

#### Extract your .epub file to a `StorageFolder`

```c#

var targetFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Dawn of Wonder - Jonathan Renshaw");

using (var stream = await ePubFile.OpenStreamForReadAsync())
{
  var archive = new ZipArchive(stream);
  archive.ExtractToDirectory(targetFolder.Path);
}

```

#### Create and initialize the Papyrus eBook

```c#

var eBook = new EBook(targetFolder);
await eBook.InitializeAsync();

```

### Papyrus HTML Parser - Getting native content

This part is optional. At this point you can get HTML content from your eBook. Papyrus HTML Parser allows you to get a collection of `Block` elements which can be displayed in a `RichTextBlock`, representing the HTML. It makes for a much cleaner layout, but could potentially misrepresent the actual layout as displayed in HTML.

#### Getting the HTML content

The `EBook.GetContentsAsync()` method has three different signatures. It can take a `NavPoint`, a `SpineItem` or a `ManifestItem`. This demo will use a `SpineItem`.

```c#

var html = await eBook.GetContentsAsync(spineItem);

```

#### Parsing the HTML

Getting the CSS styles is optional, but will help to display the UI more closely to how it is displayed in HTML. You may use whatever method you wish to get the CSS content, now that you can parse the location of the stylesheets from the HTML. The EBook class does have an extension method which takes an absolute path to a file inside its folder and returns the file. E.g.  `Book.GetFileAsync("C:/Path/To/The/EBook/styles/styles.css");` I intend to build the CSS loading functionality right into Papyrus in the future. If you opt not to get the CSS, just pass an empty string to the parser.

```c#

var converter = new Converter();
converter.Convert(html, css);

```

You now have access to a collection of `Block` elements. (`converter.ConvertedBlocks`)

### Papyrus UI - Displaying the `Block` elements

You're now ready to display your content. Papyrus has two controls to make this easier: `TableOfContents` and `Parchment`.

#### Using the `TableOfContents`

```xaml

xmlns:papyrus="using:Papyrus.UI"

<papyrus:TableOfContents x:Name="TableOfContents"
                         Parchment="{x:Bind MainParchment, Mode=OneWay}"
                         Source={x:Bind MyEBook.TableOfContents, Mode=OneWay}">
      <papyrus:TableOfContents.Header>
          <TextBlock FontSize="16"
                     FontWeight="SemiBold"
                     Margin="12"
                     Text="Table of contents" />
      </papyrus:TableOfContents.Header>
</papyrus:TableOfContents>

```

We provided a `Parchment` to the `Parchment` property. This allows the `TableOfContents` to take care of loading the correct content into the `Parchment` for us. If you can't or don't want to do this, you can tap into the `SelectionChanged` event of the `TableOfContents` instead, and take care of loading it yourself.

#### Using the `Parchment`

```xaml

xmlns:papyrus="using:Papyrus.UI"

<papyrus:Parchment x:Name="MainParchment"
                   Background="White"
                   Padding="24"
                   ParagraphIndentation="24"
                   Source="{x:Bind MyEBook, Mode=OneWay}" />

```

You're all set! you should now be able to display your eBook's content. For more advanced scenarios, check out the available properties and methods below.

## Available properties and methods

[Papyrus](#papyrus)

### Papyrus

Components in the `Papyrus` library.

#### EBook

Properties and methods in the `EBook` class.

##### Properties

**ContentLocation:** A relative path to the `content.opf` file.
