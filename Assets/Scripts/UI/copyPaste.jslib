mergeInto(LibraryManager.library, {

    CopyToClipboard: async function (str) {
        let textarea = document.createElement("textarea");
        textarea.style.opacity = "0";
        textarea.style.position = "fixed";
        textarea.value = UTF8ToString(str);
        document.body.appendChild(textarea);

        textarea.focus();
        textarea.setSelectionRange(0, textarea.value.length);
        document.execCommand("copy");
        console.log("Copy successful");

        textarea.remove();
    },

    CopyToClipboardCorrect: async function (str) {
        navigator.clipboard.writeText(UTF8ToString(str))
            .then(
                () => console.log("Copy successful"),
                () => {
                    try {
                        let textarea = document.createElement("textarea");
                        textarea.style.opacity = "0";
                        textarea.style.position = "fixed";
                        textarea.value = UTF8ToString(str);
                        document.body.appendChild(textarea);

                        textarea.focus();
                        textarea.setSelectionRange(0, textarea.value.length);
                        document.execCommand("copy");
                        console.log("Copy successful");

                        textarea.remove();
                    } catch (err) {
                        console.log(err);
                    }
                }
            );
    },
    
    ClearClipboard: function () {
        document.getElementById("input").value = "";
    },

    HideClipboard: function () {
        console.log("hide called")
        document.getElementById("input").hidden = true;
    },

    ShowClipboard: function () {
        console.log("show called")
        document.getElementById("input").hidden = false;
    },
    
    PasteFromEvent: function () {
        let textarea = document.createElement("textarea");
        textarea.style.opacity = "0";
        textarea.style.position = "fixed";
        textarea.value = "";
        textarea.addEventListener("paste", (e) => {
            
            let paste = e.clipboardData.getData("text");
            console.log("paste attempted");
            return paste;
        })
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
    },
    
    QueryPermissions: async function () {
        const queryOpts = { name: 'clipboard-read', allowWithoutGesture: false };
        const permissionStatus = await navigator.permissions.query(queryOpts);
        // Will be 'granted', 'denied' or 'prompt':
        console.log(permissionStatus.state);

        // Listen for changes to the permission state
        permissionStatus.onchange = () => {
            console.log(permissionStatus.state);
        };
    },
    
    
    
    
    

    CopyGivenText: function (text) {
    try {
            if (!navigator.clipboard) {
                // First create a textarea element to hold the given text
                // and set its content being selected.
                // A他 remove it after copping the selection to the clipboard.
    
                let textarea = document.createElement("textarea");
                textarea.style.opacity = "0";
                textarea.style.position = "fixed";
                textarea.value = text;
                document.body.appendChild(textarea);
    
                textarea.focus();
                textarea.setSelectionRange(0, textarea.value.length);
                document.execCommand("copy");
                console.log("Copy successful");
    
                textarea.remove();
            } else {
                
            }
        } catch (err) {
            console.error(err);
        }
}
    
    

    

    
});