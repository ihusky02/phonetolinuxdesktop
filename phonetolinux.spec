# Disable build-id checks for SkiaSharp prebuilt libraries
%global _build_id_links none
%define debug_package %{nil}

Name:           phonetolinuxdesktop
Version:        1.0.3
Release:        1%{?dist}
Summary:        Desktop client for PhoneToLinux integration

License:        GPL-3.0-only
URL:            https://github.com/ihusky02/phonetolinuxdesktop
Source0:        %{url}/archive/refs/tags/v%{version}.tar.gz

BuildRequires:  dotnet-sdk-8.0
Requires:       dotnet-runtime-8.0

%description
An Avalonia UI and .NET 8 desktop application for integrating and synchronizing
calls, messages, and phone notifications directly with your Linux desktop.

%prep
%autosetup -n %{name}-%{version}

%build
# Dynamically match the .NET RID based on the RPM build architecture
case "%{_arch}" in
    x86_64)
        dotnet_rid="linux-x64"
        ;;
    aarch64)
        dotnet_rid="linux-arm64"
        ;;
    armv7hl)
        dotnet_rid="linux-arm"
        ;;
    *)
        dotnet_rid="linux-%{_arch}"
        ;;
esac

# Clear default sources and add only the local NuGet packages directory
dotnet nuget add source $PWD/nupkgs --name local-packages

# Restore packages pointing directly to the local source without failing on offline switch
dotnet restore --source $PWD/nupkgs

# Publish the application for the selected architecture
dotnet publish -c Release -r $dotnet_rid --self-contained true --no-restore -o out/

%install
# Create target system directory structure
mkdir -p %{buildroot}%{_bindir}
mkdir -p %{buildroot}%{_datadir}/%{name}
mkdir -p %{buildroot}%{_datadir}/applications
mkdir -p %{buildroot}%{_datadir}/icons/hicolor/512x512/apps

# Copy published binaries to the application directory
cp -r out/* %{buildroot}%{_datadir}/%{name}/

# Create an executable symlink in /usr/bin
ln -s %{_datadir}/%{name}/phonetolinux %{buildroot}%{_bindir}/phonetolinuxdesktop

# Copy system desktop entry from root directory and PNG icon from Assets
cp phonetolinuxdesktop.desktop %{buildroot}%{_datadir}/applications/
cp Assets/icon.png %{buildroot}%{_datadir}/icons/hicolor/512x512/apps/phonetolinuxdesktop.png

%files
%{_bindir}/phonetolinuxdesktop
%{_datadir}/%{name}/
%{_datadir}/applications/phonetolinuxdesktop.desktop
%{_datadir}/icons/hicolor/512x512/apps/phonetolinuxdesktop.png

%changelog
* Fri Sep 25 2026 Stanisław Tłołka <stanislawtlolka@gmail.com> - 1.0.3-1
- Bump version to 1.0.3, add offline local NuGet packages source support.

* Wed Sep 23 2026 Stanisław Tłołka <stanislawtlolka@gmail.com> - 1.0.2-1
- Bump version to 1.0.2 and disable build-id generation for prebuilt libraries.

* Wed Sep 23 2026 Stanisław Tłołka <stanislawtlolka@gmail.com> - 1.0.1-1
- Switch to GitHub tag source tarball and bump version to 1.0.1.