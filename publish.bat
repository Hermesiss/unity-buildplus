copy README.md "Packages/trismegistus.unity.buildplus/README.md" /Y
copy CHANGELOG.md "Packages/trismegistus.unity.buildplus/CHANGELOG.md" /Y
cd Packages/trismegistus.unity.buildplus
npm publish --registry http://upm.trismegistus.tech:4873/ || pause