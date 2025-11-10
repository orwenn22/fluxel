<h1 align="center">fluxel</h1>
<p align="center"><img src="https://assets.flux.moe/avatar/318dbc829bb2978daa8b7edd6f8377f7dc549d0b51b8f068cb56779f094a3961" width="128" alt="fluxel logo"/></p>
<p align="center">The backend for fluXis.</p>

## Building and Developing

### Requirements

* A desktop computer running Windows, macOS, or Linux with
  the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.
* An IDE of your choice, for
  example [JetBrains Rider](https://www.jetbrains.com/rider/), [Visual Studio](https://visualstudio.microsoft.com/vs/)
  or [Visual Studio Code](https://code.visualstudio.com/).

### Downloading the source code

You can download the source code by cloning the repository using git:

```shell
git clone https://github.com/InventiveRhythm/fluxel
```

Make sure the [fluXis repo](https://github.com/InventiveRhythm/fluXis) is cloned next to where the fluxel repo is.

> Your filesystem should look like this if done correctly:
> ```
> ~/GitHub/InventiveRhythm> ls
> fluXis
>   fluXis/
>   fluXis.Resources/
>   fluXis.sln
>   ...
> fluxel
>   fluxel/
>   fluxel.Startup/
>   fluxel.sln
>   ...
> ```

To update the source code to the latest version, run the following command in the repository directory:

```shell
git pull
```

### Building

When running the project for the first time, make sure to copy `default.env` to `.env`.

To build and run the project, execute the following command in the repository directory (the folder where fluxel.sln is
located):

```shell
dotnet run --project fluxel.Startup
```

## License
fluxel is licensed under the [MIT License](LICENSE). tl;dr: You can do whatever you want with the code, as long as you include the original license.