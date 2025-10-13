function TaskSummary({ summary, onGenerate, loading, taskCount }) {
  return (
    <div className="task-summary-container">
      <h2>Tasks Summary</h2>

      <button
        onClick={onGenerate}
        disabled={loading || taskCount === 0}
        className="summary-btn"
      >
        {loading ? "Generating summary..." : "Generate Summary with AI"}
      </button>

      {taskCount === 0 && (
        <p className="summary-empty">
          Add some tasks to generate a summary!
        </p>
      )}

      {summary && (
        <div className="summary-content">
          <h3>Summary:</h3>
          <div className="summary-text">
            {summary.split("\n").map((line, index) => (
              <p key={index}>{line}</p>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default TaskSummary;
