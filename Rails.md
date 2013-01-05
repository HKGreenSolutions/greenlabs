

```ruby
class CreateQuizzesQuestions < ActiveRecord::Migration
  def up

    create_table :quizzes_questions, :id=> false do |t|
      t.integer :quiz_id, :null => false
      t.integer :question_id, :null => false
    end

    add_index :quizzes_questions, [:quiz_id, :question_id], :unique => true

  end

  def down
    remove_index :quizzes_questions, :column => [:quiz_id, :question_id]
    drop_table :quizzes_questions
  end
end
```
