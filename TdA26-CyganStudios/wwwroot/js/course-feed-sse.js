let eventSource;

function startCourseFeed(courseId, maxPreviewCount, editable) {
    if (eventSource) {
        eventSource.close();
    }

    const container = document.getElementById("feed-container");
    const viewAllCard = document.getElementById("view-all-card");
    const noActivityYetCard = document.getElementById("no-activity-yet-card");

    eventSource = new EventSource(`/api/courses/${courseId}/feed/stream`);

    eventSource.addEventListener("new_post", (event) => {
        const data = JSON.parse(event.data);
        if (noActivityYetCard != null) {
            noActivityYetCard.style.display = "none";
        }
        prependFeedItem(data, container, viewAllCard, maxPreviewCount, editable, courseId);
    });

    eventSource.onerror = (err) => {
        console.warn("SSE error", err);
    };
}

function prependFeedItem(post, container, viewAllCard, maxPreviewCount, editable, courseId) {
    if (!container) return;

    const card = document.createElement("div");
    card.className = "card feed-item position-relative";
    card.dataset.uuid = post.uuid;

    const isManual = !post.isSystemMessage;
    const message = post.isSystemMessage ? post.message : escapeHtml(post.message);

    const editDropdown = (editable && isManual) ? `
        <div class="dropdown position-absolute top-0 end-0 m-2">
            <button class="btn btn-sm bg-body-secondary p-1 text-muted"
                    type="button"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                    style="line-height: 1;">
                <i class="bi bi-three-dots-vertical"></i>
            </button>

            <ul class="dropdown-menu dropdown-menu-end">
                <li>
                    <a class="dropdown-item"
                       href="/dashboard/course/${courseId}/feed/${post.uuid}/edit">
                        <i class="bi bi-pencil-square"></i> Edit
                    </a>
                </li>
                <li>
                    <form method="post"
                          action="/dashboard/course/${courseId}?itemUuid=${post.uuid}&type=feed-item&handler=Delete"
                          onsubmit="return confirm('Are you sure you want to delete this post?');">
                        <button type="submit" class="dropdown-item text-danger">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </form>
                </li>
            </ul>
        </div>
    ` : "";

    const editedBadge = post.edited
        ? `<span class="badge bg-light text-muted ms-1">edited</span>`
        : "";

    card.innerHTML = `
        ${editDropdown}

        <div class="card-body py-2">
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <span class="badge bg-secondary me-2">${post.isSystemMessage ? "System" : "Manual"}</span>
                    ${editedBadge}
                </div>

                <small class="text-muted">
                    ${post.createdAt}
                </small>
            </div>

            <div class="mt-2">
                ${message}
            </div>
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
