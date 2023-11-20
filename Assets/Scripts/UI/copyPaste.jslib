mergeInto(LibraryManager.library, {

    CopyToClipboard: async function (str) {
        await navigator.clipboard.writeText(UTF8ToString(str));
    },
    
    PasteFromClipboard: function () {
        return UTF8ToString(navigator.clipboard.readText());
    },
    
    HandlePermission: function (str) {
      navigator.permissions.query({ name: UTF8ToString(str) }).then((result) => {
        if (result.state === "granted") {
          console.log(result.state);
        } else if (result.state === "prompt") {
          console.log(result.state);
        } else if (result.state === "denied") {
          console.log(result.state);
        }
        result.addEventListener("change", () => {
          console.log("XD")
        });
      });
    }
});