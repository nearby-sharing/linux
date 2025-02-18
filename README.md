# Nearby Sharing for Linux 🎉
This is a small gtk 4 (+ libadwaita) gui allowing you to send and receive files and urls to and from Windows 10 / 11 devices using the [Connected Devices Protocol](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-cdp).

> [!TIP]
> You don't need to install anything on Windows as this is the native Windows Nearby Sharing protocol.

> [!WARNING]
> This app is still in preview and might be unstable.   
> You have to build it from source.

## Development
- Clone repo (with submodules!)
  ```sh
  git clone --recurse-submodules git@github.com:nearby-sharing/linux.git
  ```
- Install [.NET 9 SDK](https://dot.net)
- `dotnet run src`

## License
This Project is licensed under [GPL-3.0](LICENSE.txt).
