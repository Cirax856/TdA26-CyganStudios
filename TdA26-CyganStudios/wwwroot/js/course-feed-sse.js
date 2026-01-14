let eventSource;

function startCourseFeed(courseId) {
    if (eventSource) {
        eventSource.close();
    }

    eventSource = new EventSource(`/api/feed/stream?courseId=${courseId}`);

    eventSource.addEventListener("new_post", (event) => {
        const data = JSON.parse(event.data);
        prependFeedItem(data);
    });

    eventSource.onerror = (err) => {
        console.warn("SSE error", err);
    };
}

function prependFeedItem(post) {
    const container = document.getElementById("feed-container");
    if (!container) return;

    const card = document.createElement("div");
    card.className = "card feed-item";
    card.dataset.uuid = post.uuid;

    card.innerHTML = `
        <div class="card-body py-2">
            <div class="d-flex justify-content-between align-items-start">
                <span class="badge bg-secondary">System</span>
                <small class="text-muted">just now</small>
            </div>
            <div class="mt-2">${escapeHtml(post.message)}</div>
        </div>
    `;

    container.prepend(card);
}

function escapeHtml(str) {
    const div = document.createElement("div");
    div.innerText = str;
    return div.innerHTML;
}
