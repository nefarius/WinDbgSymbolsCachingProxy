window.symbolsAuthRetry = {
    trigger: async function () {
        try {
            const response = await fetch("/auth-challenge", {
                method: "GET",
                credentials: "include",
                cache: "no-store"
            });

            return response.ok === true;
        } catch (_) {
            return false;
        }
    }
};
