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
rake db:drop
rake db:create
rake db:migrate

rails g model quiz title:string category:string overview:text author:string
rails g model question qtext:text type:string
rails g model answer atext:string correct:boolean question:references

# Create a many-to-many relation between quizzes and questions
rails g migration CreateQuizzesQuestions

```


```ruby
class CreateQuestionsQuizzes < ActiveRecord::Migration
  def up
    create_table :questions_quizzes, :id=> false do |t|
      t.integer :question_id, :null => false
      t.integer :quiz_id, :null => false
    end
    add_index :questions_quizzes, [:question_id, :quiz_id], :unique => true
  end
  def down
    remove_index :questions_quizzes, :column => [:question_id, :quiz_id]
    drop_table :questions_quizzes
  end
end
```


* Reference

http://blogs.sussex.ac.uk/elearningteam/2012/08/29/improving-moodle-import-part-1-the-database-schema/
