let eventSource;

function startCourseFeed(courseId, maxPreviewCount) {
    if (eventSource) {
        eventSource.close();
    }

    const container = document.getElementById("feed-container");
    const viewAllCard = document.getElementById("view-all-card");

    eventSource = new EventSource(`/api/courses/${courseId}/feed/stream`);

    eventSource.addEventListener("new_post", (event) => {
        const data = JSON.parse(event.data);
        prependFeedItem(data, container, viewAllCard, maxPreviewCount);
    });

    eventSource.onerror = (err) => {
        console.warn("SSE error", err);
    };
}

function prependFeedItem(post, container, viewAllCard, maxPreviewCount) {
    if (!container) return;

    const card = document.createElement("div");
    card.className = "card feed-item";
    card.dataset.uuid = post.uuid;

    const message = post.isSystemMessage ? post.message : escapeHtml(post.message);
    card.innerHTML = `
        <div class="card-body py-2">
            <div class="d-flex justify-content-between align-items-start">
                <span class="badge bg-secondary">System</span>
                <small class="text-muted">${post.createdAt}</small>
            </div>
            <div class="mt-2">${message}</div>
        </div>
    `;

    container.prepend(card);

    const items = Array.from(container.querySelectorAll(".feed-item"));
    if (items.length > maxPreviewCount - 1) {
        for (let i = maxPreviewCount - 1; i < items.length; i++) {
            items[i].remove();
        }
    }

    if (viewAllCard) {
        viewAllCard.style.display = "block";
    }
}

function escapeHtml(str) {
    const div = document.createElement("div");
    div.innerText = str;
    return div.innerHTML;
}
