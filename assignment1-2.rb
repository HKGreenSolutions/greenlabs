# Assignment 1-2. Object-oriented
q1=['1. Where is the capital of China?','A. Beijing','B. Shanghai','C. Guangzhou',"D. Hong Kong\n"]
q2=['2. Where is the capital of Japan?','A. Osaka','B. Fukuoka','C. Kagawa','D. Okayama','E. Ishikawa',"F. Tokyo\n"]
q3=['3. Which of following are functional language?','A. Lisp','B. Racket','C. Java','D. PHP',"E. Scheme\n"]
questions=[q1,q2,q3]
$n=questions.size
$standard_answer=["A\n","F\n","ABE\n"]
$reuse=['Your answer: ','The correct answer: ']
your_answer=[]

class Quiz
  def initialize(question,answer)
    @question=question
    @answer=answer
  end
  def show_questions
    for i in 0..$n-1 do
      puts @question[i]
      puts ""
      print $reuse[0]
      @answer[i]=gets
      puts ""
    end
  end
  def show_answers
    puts "Done Exam"
    puts "="*40
    puts "Result Slip"
    puts "="*40
    for i in 0..$n-1 do
      puts @question[i][0]
      puts $reuse[0] + @answer[i]
      puts $reuse[1] + $standard_answer[i]
      puts ""
    end
  end
  def check_answers
    right_answer = @answer & $standard_answer
    m = format("%0.2f", right_answer.size.to_f/$n.to_f*100)
    print 'Your score:',right_answer.size,'/',$n,'(',m,'%)'
  end
end

quiz1=Quiz.new(questions,your_answer)
quiz1.show_questions
quiz1.show_answers
quiz1.check_answers