{
  description = "PiggCrapp Flake File";

  inputs = {

    flake-utils = {
      url = "github:numtide/flake-utils";
    };

    nixpkgs = {
      url = "nixpkgs/nixos-unstable";
    };

  };

  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
      in {
        devShell = pkgs.mkShell {
          buildInputs = with pkgs; [
            (with dotnetCorePackages; combinePackages [
              dotnet-sdk_6
              dotnetPackages.Nuget
            ])
          ];
        };
      });
}
