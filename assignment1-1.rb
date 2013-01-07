q1=['1. Where is the capital of China?','A. Beijing','B. Shanghai','C. Guangzhou',"D. Hong Kong\n"]
q2=['2. Where is the capital of Tokyo?','A. Osaka','B. Fukuoka','C. Kagawa','D. Okayama','E. Ishikawa',"F. Tokyo\n"]
q3=['3. Which of following are functional language?','A. Lisp','B. Racket','C. Java','D. PHP',"E. Scheme\n"]
reuse=['Your answer:','The correct answer:']
your_answer=[]
standard_answer=["A\n","E\n","ABE\n"]
questions=[q1,q2,q3]
n=questions.size

i=0
while i<= n-1
  puts questions[i], "\n"
  print reuse[0]
  your_answer[i]=gets
  puts ""
  i=i+1
end

puts "Done Exam"
puts "="*40
puts "Result Slip"
puts "="*40

j=0
while j<=n-1
  puts questions[j][0]
  puts reuse[0] + your_answer[j]
  puts reuse[1] + standard_answer[j] + "\n"
  j=j+1
end


#questions.each_with_index { |q, j|
#  puts j.to_s + "  @@" + q[0]
#}

right_answer = your_answer & standard_answer
m = format("%0.2f", right_answer.size.to_f/n.to_f*100)
print 'Your score:',right_answer.size,'/',n,'(',m,'%)'