window.symbolsProxy = {
    copyText: async function (text) {
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(text);
                return true;
            }
        } catch (_) {
        }

        const previousFocus = document.activeElement;
        const textarea = document.createElement("textarea");
        textarea.value = text;
        textarea.setAttribute("readonly", "");
        textarea.style.position = "fixed";
        textarea.style.top = "0";
        textarea.style.left = "0";
        textarea.style.width = "1px";
        textarea.style.height = "1px";
        textarea.style.opacity = "0";
        document.body.appendChild(textarea);

        try {
            textarea.focus();
            textarea.select();
            textarea.setSelectionRange(0, text.length);
            return document.execCommand("copy") === true;
        } catch (_) {
            return false;
        } finally {
            if (textarea.parentNode) {
                textarea.parentNode.removeChild(textarea);
            }

            if (previousFocus && typeof previousFocus.focus === "function") {
                previousFocus.focus();
            }
        }
    }
};
