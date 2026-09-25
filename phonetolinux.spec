# Disable build-id checks for SkiaSharp prebuilt libraries
%global _build_id_links none
%define debug_package %{nil}

# Exclude unwanted automatic requirements (like missing lttng-ust in newer Fedora versions)
%global __requires_exclude ^liblttng-ust\\.so\\.0.*$

Name:           phonetolinuxdesktop
Version:        1.0.3
Release:        1%{?dist}
Summary:        Desktop client for PhoneToLinux integration

License:        GPL-3.0-only
URL:            https://github.com/ihusky02/phonetolinuxdesktop
Source0:        %{url}/archive/refs/tags/v%{version}.tar.gz

BuildRequires:  dotnet-sdk-8.0

# System runtime & GUI dependencies required by Avalonia UI / SkiaSharp on Linux
Requires:       dotnet-runtime-8.0
Requires:       fontconfig
Requires:       libX11
Requires:       mesa-libGL
Requires:       libICE
Requires:       libSM

%description
An Avalonia UI and .NET 8 desktop application for integrating and synchronizing
calls, messages, and phone notifications directly with your Linux desktop.

%prep
%autosetup -p1

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

# Restore packages with architecture mapping and publish application
dotnet restore -r $dotnet_rid
dotnet publish -c Release -r $dotnet_rid --self-contained true --no-restore -o out/

%install
# Create target system directory structure
mkdir -p %{buildroot}%{_bindir}
mkdir -p %{buildroot}%{_datadir}/%{name}
mkdir -p %{buildroot}%{_datadir}/applications
mkdir -p %{buildroot}%{_datadir}/icons/hicolor/512x512/apps

# Copy published binaries to the application directory
cp -r out/* %{buildroot}%{_datadir}/%{name}/

# Create a relative executable symlink in /usr/bin (fixing the absolute symlink warning)
ln -s ../share/%{name}/phonetolinux %{buildroot}%{_bindir}/phonetolinuxdesktop

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
- Clean rebuild setup with online NuGet restore, filtered lttng-ust dependency, and added graphics/font system requirements.