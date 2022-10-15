mergeInto(LibraryManager.library, {

  RequestMemorableNFT: function (score, token) {
      window.postMessage({type: "mint-memorable-nft", score: score, token: token}, "https://board.stacksforce.xyz");
  }

});