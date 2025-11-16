(function () {

    const root = document.documentElement;
    const toggleBtn = document.getElementById("theme-toggle");
    const icon = document.getElementById("theme-icon");

    // Load stored preference
    const savedTheme = localStorage.getItem("theme");

    if (savedTheme === "light" || savedTheme === "dark") {
        root.setAttribute("data-theme", savedTheme);
    } else {
        // Default to device preference
        const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        root.setAttribute("data-theme", prefersDark ? "dark" : "light");
    }

    updateIcon();

    toggleBtn?.addEventListener("click", () => {
        const current = root.getAttribute("data-theme");
        const newTheme = current === "dark" ? "light" : "dark";

        root.setAttribute("data-theme", newTheme);
        localStorage.setItem("theme", newTheme);

        updateIcon();
    });

    function updateIcon() {
        const theme = root.getAttribute("data-theme");
        icon.className = theme === "dark" ? "bi bi-sun" : "bi bi-moon";
    }

})();
