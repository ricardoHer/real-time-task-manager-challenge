import TaskItem from "./TaskItem";

function TaskList({ tasks }) {
  if (tasks.length == 0) {
    return (
      <div className="task-list-container">
        <h2>Tasks</h2>
        <div className="empty-state">
          <p>No tasks yet</p>
          <p>Create your first task above</p>
        </div>
      </div>
    );
  }

  return (
    <div className="task-list-container">
      <h2>Tasks ({tasks.length})</h2>
      <div className="task-list">
        {tasks.map((task) => (
          <TaskItem key={task.id} task={task} />
        ))}
      </div>
    </div>
  )
}


export default TaskList