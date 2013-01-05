
Experience:

05 Jan 2013

Build up the Quizzes <=> Questions <=> Answer

qtext


Quizzes:
- Name
- Author

Questions:
- qtext:text
- type:string {choice, freeinput}


Answers:
- atext:string
- correct:boolean
- question belongs to: references
TODO: review a question

```sh
rails g model quiz title:string category:string overview:text author:string
rails g model question qtext:text type:string
rails g model answer atext:string correct:boolean question:references

# Create a many-to-many relation between quizzes and questions
rails g migration CreateQuizzesQuestions

```


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
