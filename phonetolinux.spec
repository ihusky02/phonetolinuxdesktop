Name:           phonetolinuxdesktop
Version:        1.0.0
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
# Publish the .NET application as a framework-dependent Release build
dotnet publish -c Release -o out/

%install
# Create target system directory structure
mkdir -p %{buildroot}%{_bindir}
mkdir -p %{buildroot}%{_datadir}/%{name}
mkdir -p %{buildroot}%{_datadir}/applications
mkdir -p %{buildroot}%{_datadir}/icons/hicolor/scalable/apps

# Copy published binaries to the application directory
cp -r out/* %{buildroot}%{_datadir}/%{name}/

# Create an executable symlink in /usr/bin
ln -s %{_datadir}/%{name}/PhoneToLinuxDesktop %{buildroot}%{_bindir}/phonetolinuxdesktop

# Copy system desktop entry and scalable SVG icon
cp assets/phonetolinuxdesktop.desktop %{buildroot}%{_datadir}/applications/
cp assets/phonetolinuxdesktop.svg %{buildroot}%{_datadir}/icons/hicolor/scalable/apps/phonetolinuxdesktop.svg

%files
%{_bindir}/phonetolinuxdesktop
%{_datadir}/%{name}/
%{_datadir}/applications/phonetolinuxdesktop.desktop
%{_datadir}/icons/hicolor/scalable/apps/phonetolinuxdesktop.svg

%changelog
* Wed Sep 23 2026 Stanisław Tlołka <stanislawtlolka@gmail.com> - 1.0.1
- Add autodetect IP Adress in File Browser