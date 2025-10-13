import { useState } from "react";


function TaskForm({ onSubmit }) {

    const [title, setTitle] = useState('')
    const [description, setDescription] = useState('')
    const [submitting, setSubmitting] = useState(false)

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!title.trim() || !description.trim()) {
            alert('Please, fill out every field')
            return
        }

        setSubmitting(true)

        try {
            await onSubmit({
                title: title.trim(),
                description: description.trim()
            })

            // Cleaning the form
            setTitle('')
            setDescription('')
        } catch(error) {
            console.error('error on submitting task', error)
        } finally {
            setSubmitting(false)
        }
    }

    return (
    <div className="task-form-container">
      <h2>➕ New Task</h2>
      <form onSubmit={handleSubmit} className="task-form">
        <div className="form-group">
          <label htmlFor="title">Title:</label>
          <input
            id="title"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Inform the title..."
            disabled={submitting}
            maxLength={200}
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="description">Description:</label>
          <textarea
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Describe the description..."
            disabled={submitting}
            maxLength={1000}
            rows={4}
          />
        </div>
        
        <button 
          type="submit" 
          disabled={submitting || !title.trim() || !description.trim()}
          className="submit-btn"
        >
          {submitting ? 'Adding...' : 'Add new Task'}
        </button>
      </form>
    </div>
  )
}

export default TaskForm