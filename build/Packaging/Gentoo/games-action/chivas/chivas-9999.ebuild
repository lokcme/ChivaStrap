EAPI=8

inherit desktop git-r3

DESCRIPTION="Roblox bootstrapper for Linux, fork of Bloxstrap (launches Roblox through Sober)"
HOMEPAGE="https://github.com/lokcme/ChivaStrap"

EGIT_REPO_URI="https://github.com/lokcme/ChivaStrap.git"
EGIT_BRANCH="main"
EGIT_SUBMODULES=()

SRC_URI="
	https://github.com/bloxstraplabs/wpfui/archive/f710123e72d9dcc8d09fccc4e2a783cc5cf5e652.tar.gz
"

LICENSE="MIT"
SLOT="0"
KEYWORDS=""

RESTRICT="network-sandbox mirror strip"

BDEPEND="
	>=dev-dotnet/dotnet-sdk-bin-10.0.300
"

RDEPEND="
	media-libs/vulkan-loader
	media-libs/mesa[vulkan]
	x11-libs/libX11
	x11-libs/libXext
	x11-libs/libXrender
	x11-libs/libXrandr
	x11-libs/libXi
	x11-libs/libXcursor
	x11-libs/libXfixes
	x11-libs/libXcomposite
	x11-libs/libXdamage
	x11-libs/libICE
	x11-libs/libSM
	media-libs/fontconfig
	media-libs/freetype
	x11-libs/libxkbcommon[X]
	dev-libs/wayland
	sys-apps/dbus
	virtual/opengl
	dev-libs/openssl:0
	sys-apps/flatpak
	app-misc/ca-certificates
	sys-libs/zlib
"

pkg_setup() {
	export DOTNET_CLI_TELEMETRY_OPTOUT=1
	export DOTNET_NOLOGO=1
	export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
	export MSBUILDDISABLENODEREUSE=1
	export DOTNET_CLI_HOME="${T}/dotnet-home"

	mkdir -p "${DOTNET_CLI_HOME}" || die

	case "${CHOST}:${ARCH}" in
		*-linux-gnu*:amd64)
			CHIVAS_RID="linux-x64"
			;;

		*-linux-gnu*:arm64)
			CHIVAS_RID="linux-arm64"
			;;

		*-linux-musl*:amd64)
			CHIVAS_RID="linux-musl-x64"
			;;

		*-linux-musl*:arm64)
			CHIVAS_RID="linux-musl-arm64"
			;;

		*)
			die "Unsupported platform: CHOST=${CHOST}, ARCH=${ARCH}"
			;;
	esac

	einfo "CHOST: ${CHOST}"
	einfo "ARCH: ${ARCH}"
	einfo "Chivas RID: ${CHIVAS_RID}"
}

src_compile() {
	dotnet publish \
		src/Chivas.Cross/Chivas.Cross.csproj \
		-c Release \
		-r "${CHIVAS_RID}" \
		--self-contained true \
		-o "${S}/build/publish" \
		-p:DebugType=None \
		-p:DebugSymbols=false \
		|| die "dotnet publish failed"
}

src_install() {
	dodir /usr/lib/chivas
	cp -R build/publish/. "${D}/usr/lib/chivas/" || die

	fperms 755 /usr/lib/chivas/Chivas

	dosym /usr/lib/chivas/Chivas /usr/bin/chivas

	domenu build/Packaging/Linux/chivas.desktop

	newicon -s 256 \
		src/Chivas.App/Chivas.png \
		io.github.lokcme.Chivas.png

	insinto /usr/share/metainfo
	doins build/Packaging/Linux/io.github.lokcme.Chivas.metainfo.xml

	dodoc \
		LICENSE.CHIVAS \
		LICENSE.BLOXSTRAP \
		LICENSE.FISHSTRAP \
		README.md
}
