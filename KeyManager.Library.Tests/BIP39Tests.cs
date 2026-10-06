namespace Leosac.KeyManager.Library.Tests
{
    /// <summary>
    /// Mnemonic tests.
    /// Based on https://github.com/elucidsoft/dotnetstandard-bip39 (Apache License 2.0)
    /// </summary>
    [TestClass]
    public class BIP39Tests
    {
        [TestMethod]
        public void TestEnglish_Test1()
        {
            var entropy = "00000000000000000000000000000000";
            var mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            var seedHex = "a3c9324fab99733ef7b5f9313e49cd45a134ee77da9aa176a84458d4b73e46f00a59361709d71d6b68338c957366937942b8f2ce2bd306b4ab0ae63c6b4ff7f5";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test2()
        {
            var entropy = "7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f";
            var mnemonic = "legal winner thank year wave sausage worth useful legal winner thank yellow";
            var seedHex = "f48f158e9689580f91ae0c813f4353ba2f90cd242e811ec27b2d79f97eb21fcfaf54a0b546c27c3b0a60285df2b064b3eef84fa7269d93ab013af10a20adc758";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test3()
        {
            var entropy = "80808080808080808080808080808080";
            var mnemonic = "letter advice cage absurd amount doctor acoustic avoid letter advice cage above";
            var seedHex = "7804de4eb059484ebd7c384e3bd99654763ff1f736abf4e8a5053b12594ea7389beefd967031f03416fa03994cf6be7d3f2fbe37709a27cd091f9e3d310f16d0";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test4()
        {
            var entropy = "ffffffffffffffffffffffffffffffff";
            var mnemonic = "zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo wrong";
            var seedHex = "ce2467164ca78cbce7c368bf35a390474d0e3af6afc4cc47214d215dcfc52ee567fd9921e66c32fc34e8f37abc468e7ba6cc744a96e815c95c583ad849d1c372";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test5()
        {
            var entropy = "000000000000000000000000000000000000000000000000";
            var mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon agent";
            var seedHex = "e6fa0d745f5b841a4c90f1487dea80d5e5cc64c46c7aebe115b10ac333066f37b6d34ae514b8a1acb21b38c3836c248aec81c10874c2323bee98e70443cb2e2c";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test6()
        {
            var entropy = "7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f";
            var mnemonic = "legal winner thank year wave sausage worth useful legal winner thank year wave sausage worth useful legal will";
            var seedHex = "b829d8809875e7cae49114971e6ab87f13bd133c8d676972fb7a7ce8a9604b57759137091f21018ecd757906331c7e5466c8d9bc31b705e96e959beba111acc5";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test7()
        {
            var entropy = "808080808080808080808080808080808080808080808080";
            var mnemonic = "letter advice cage absurd amount doctor acoustic avoid letter advice cage absurd amount doctor acoustic avoid letter always";
            var seedHex = "e8716279b6e74e876477061b83c4b29eeab288ea85ea196d95281dc0504c3edb1c70c86042e66f56ad27fb93d24c0a64294eac9668f0bba6d13a13882de542dd";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test8()
        {
            var entropy = "ffffffffffffffffffffffffffffffffffffffffffffffff";
            var mnemonic = "zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo when";
            var seedHex = "417b80e3489de48dc644d9bf8e32ed907f918efdaf78541c219e6e1b6e7e17ed38448e7d2ef311ad4446f8be779507d288285eb5eff32ce723d4b3692a26d6a4";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test9()
        {
            var entropy = "0000000000000000000000000000000000000000000000000000000000000000";
            var mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon art";
            var seedHex = "c716b0c6e051b60f819ba04bc5c402ac15e3aa4071c99735dfecce05d41b550155ae3511789a0ccf2cb245050f53e48292e8d88864e7c46eaa2fe626373e6fc8";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test10()
        {
            var entropy = "7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f7f";
            var mnemonic = "legal winner thank year wave sausage worth useful legal winner thank year wave sausage worth useful legal winner thank year wave sausage worth title";
            var seedHex = "8d80646b3677f02a56efc2977dc5972dadddd96dc34309119fbd44f5f939344df0e6c0aa924fabfe858c123283f904e1fcb24a5e20b70d69ec82a40e3e010c12";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test11()
        {
            var entropy = "8080808080808080808080808080808080808080808080808080808080808080";
            var mnemonic = "letter advice cage absurd amount doctor acoustic avoid letter advice cage absurd amount doctor acoustic avoid letter advice cage absurd amount doctor acoustic bless";
            var seedHex = "fd544f531a25958d2980925b10e7146290dd8615fd9241a66b0681cf3afb44e607a279cc88fa4afe65779b07b3ba441dcfb13be42266642c474fb4787683556c";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test12()
        {
            var entropy = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
            var mnemonic = "zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo zoo vote";
            var seedHex = "024a5866efbd866885915e8eb2fc743af294c1b69285d2d62efff4228d6b3eb53bfc2ecd97048fc9691b200ee4f11af4e51fab23aa0ac674dd187235b4e3283f";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test13()
        {
            var entropy = "77c2b00716cec7213839159e404db50d";
            var mnemonic = "jelly better achieve collect unaware mountain thought cargo oxygen act hood bridge";
            var seedHex = "e1996663a64664965fd365680a4947b6cad71df8e1283a1d7e3b90a941c53cdb1133e1e537023b016ab56fbd650cafce2b774bc5e4dde81cc12778fe5a20cfce";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test14()
        {
            var entropy = "b63a9c59a6e641f288ebc103017f1da9f8290b3da6bdef7b";
            var mnemonic = "renew stay biology evidence goat welcome casual join adapt armor shuffle fault little machine walk stumble urge swap";
            var seedHex = "5181f94e38c3dd1d14d374ddc0c7f031304479167b9581c29ade76ebf3eebd86ab1323c4bfa938c780227f8adb4584b6b7032cbc8705560fd785c6dbaf12227d";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test15()
        {
            var entropy = "3e141609b97933b66a060dcddc71fad1d91677db872031e85f4c015c5e7e8982";
            var mnemonic = "dignity pass list indicate nasty swamp pool script soccer toe leaf photo multiply desk host tomato cradle drill spread actor shine dismiss champion exotic";
            var seedHex = "7c6c73344ebad46b0666c77b6bba457d7f9eac00bfee98d94a5dc1c5e5feadf730a54b2bc3742decba52891b1891797af444354989b9e78c601842f6206bbb16";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test16()
        {
            var entropy = "0460ef47585604c5660618db2e6a7e7f";
            var mnemonic = "afford alter spike radar gate glance object seek swamp infant panel yellow";
            var seedHex = "a689d6c9941bba3d00d61d9c5310e85ea2efb3a51fa6dd22d828fceb52310936e2b5cb4d8d9c9bf8f2f86f8c9d34618f533c84f450c8f43db4b7cd7e5297b7eb";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test17()
        {
            var entropy = "72f60ebac5dd8add8d2a25a797102c3ce21bc029c200076f";
            var mnemonic = "indicate race push merry suffer human cruise dwarf pole review arch keep canvas theme poem divorce alter left";
            var seedHex = "4724efb17d272f3b0bcde804b09e8d159a5c2b69d98fc818297e462eacc4499203a6bdd4d7269450a9478f54c7419c236286b63712f56089c9a127d8b5c4a5c6";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test18()
        {
            var entropy = "2c85efc7f24ee4573d2b81a6ec66cee209b2dcbd09d8eddc51e0215b0b68e416";
            var mnemonic = "clutch control vehicle tonight unusual clog visa ice plunge glimpse recipe series open hour vintage deposit universe tip job dress radar refuse motion taste";
            var seedHex = "37bd81bd2de8448df0e503445d9c13194c71e8764255aaf58f4621455496d690bfe5293908580c843a08ee53f8e4cadc45a8a291f99f732604519777ddc69d09";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test19()
        {
            var entropy = "eaebabb2383351fd31d703840b32e9e2";
            var mnemonic = "turtle front uncle idea crush write shrug there lottery flower risk shell";
            var seedHex = "55f248365f84af171391b28a6b7e624cdba183c2aa10211d6080501b118f1da20deafe1bcbf53905ddbd11761ae3c3da9d56c81ccdc9a8621671f89b19fc39c8";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test20()
        {
            var entropy = "7ac45cfe7722ee6c7ba84fbc2d5bd61b45cb2fe5eb65aa78";
            var mnemonic = "kiss carry display unusual confirm curtain upgrade antique rotate hello void custom frequent obey nut hole price segment";
            var seedHex = "d5137949a621d12bf66fd7cc7bdb5c805d1d0338f68aac38c46d4874d106b7405533db0f30c0c68c53051a3d76a010cc9dd21de6423cf50be2f585ae53263b74";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test21()
        {
            var entropy = "18ab19a9f54a9274f03e5209a2ac8a91";
            var mnemonic = "board flee heavy tunnel powder denial science ski answer betray cargo cat";
            var seedHex = "ff7cb58126be7c20aec0c360276c7357976ee24c24970498bfce581746e9660af4b1a58b790033753aff6fdb352d059978a67fe9aa81829a20661618bab7a2fb";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test22()
        {
            var entropy = "18a2e1d81b8ecfb2a333adcb0c17a5b9eb76cc5d05db91a4";
            var mnemonic = "board blade invite damage undo sun mimic interest slam gaze truly inherit resist great inject rocket museum chief";
            var seedHex = "8c4dcd921f75fdf4ad1e0bfcf2a1df53779803261d0c6973e36bb3c46ff8637df19a0b059a9454fbeaeb70938cf53f98345baf3d2e90e52d36caecdaf0e7497a";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }

        [TestMethod]
        public void TestEnglish_Test23()
        {
            var entropy = "15da872c95a13dd738fbf50e427583ad61f18fd99f628c417a61cf8343c90419";
            var mnemonic = "beyond stage sleep clip because twist token leaf atom beauty genius food business side grid unable middle armed observe pair crouch tonight away coconut";
            var seedHex = "20d781d15345cbca14441e753b32fd52fa59fc1569662e89c6957b947605a1fdab8aa7f7a1e19dec0c5649866ad974eeffef79b2875d32678d060b92ec69b295";

            var result = TestVector(KeyGen.Mnemonic.WordlistLang.English, "TREZOR", entropy, mnemonic);

            Assert.AreEqual(result.entropy.ToLowerInvariant(), entropy);
            Assert.AreEqual(result.seedHex.ToLowerInvariant(), seedHex);
            Assert.AreEqual(result.mnemonic, mnemonic);
        }


        private static (string entropy, string seedHex, string mnemonic) TestVector(KeyGen.Mnemonic.WordlistLang wordList, string password,
            string entropy, string mnemonic)
        {
            var bip39 = new KeyGen.Mnemonic.BIP39();
            var entropyResult = bip39.MnemonicToEntropy(mnemonic, wordList);
            var seedResult = bip39.MnemonicToSeedHex(mnemonic, password);
            var mnemonicResult = bip39.EntropyToMnemonic(entropy, wordList);

            return (entropyResult, seedResult, mnemonicResult);
        }
    }
}