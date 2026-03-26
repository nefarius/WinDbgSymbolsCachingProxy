window.symbolsAuthRetry = {
    trigger: async function (returnUrl) {
        try {
            const response = await fetch("/api/auth/challenge", {
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
