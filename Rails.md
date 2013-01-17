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


```ruby
questions = 
  Question.create(
                  [{qtext: 'Where is capital of China', qtype: 'MC'},
                   {qtext: 'Where is capital of United State', qtype: 'MC'},
                   {qtext: 'Where is capital of Canada', qtype: 'MC'},
                   {qtext: 'Where is capital of Japan', qtype: 'MC'}])

q1_ans =
  Answer.create(
                [{ atext: 'Beijing', correct: true},
                 { atext: 'Hong Kong', correct: false},
                 { atext: 'GuanZhou', correct: false},
                 { atext: 'Shanghai', correct: false}])
                 
q2_ans =
  Answer.create(
                [{ atext: 'Washington', correct: true},
                 { atext: 'New York', correct: false},
                 { atext: 'New Jersy', correct: false},
                 { atext: 'Los Angels', correct: false}])

q3_ans =
  Answer.create(
                [{ atext: 'Ottawa', correct: true},
                 { atext: 'Quebec', correct: false},
                 { atext: 'Waterloo', correct: false},
                 { atext: 'Vancourer', correct: false}])

q4_ans = 
  Answer.create(
                [{ atext: 'Hoikaido', correct: false},
                 { atext: 'Osaka', correct: false},
                 { atext: 'Tokyo', correct: true},
                 { atext: 'Fukuo', correct: false}])

#Due to seed's inability to initialize by massively create reference key on question_id
                 
ans = [q1_ans, q2_ans, q3_ans, q4_ans]

ans.each_with_index do |a, i|
  a.each do |aa|
    aa.question_id=i
    aa.save
  end
end

```

* Reference

http://blogs.sussex.ac.uk/elearningteam/2012/08/29/improving-moodle-import-part-1-the-database-schema/
