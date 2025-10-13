function TaskItem({ task }) {
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="task-item">
      <div className="task-header">
        <h3 className="task-title">{task.title}</h3>
        <span className="task-date">{formatDate(task.createdAt)}</span>
      </div>
      <p className="task-description">{task.description}</p>
      <div className="task-id">ID: {task.id}</div>
    </div>
  );
}

export default TaskItem;
