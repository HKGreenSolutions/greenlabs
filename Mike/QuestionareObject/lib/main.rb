# To change this template, choose Tools | Templates
# and open the template in the editor.

class Question
  $ans1=Array.new()
  def exam(n)
    ques="1. Where is the capital of China?
    " , "2. Where is the capital of Japan?
    ","3. Which of following are functional language?"
    ansl="A. Beijing\nB. Shanghai\nC. Guangzhou\nD. Hong Kong
    ","A. Osaka\nB. Fukuoka\nC. Kagawa\nD. Okayama\nE. Ishikawa\nF. Tokyo
    ","A. Lisp\nB. Racket\nC. Java\nD. PHP\nE. Scheme"
    anst="A","F","ABE"
    
  if n == ques.length
    puts "Done Exam\n=============================================\nResult Slip\n============================================="
    count = 0
    for i in 0..ques.length-1
      puts "\n" + ques[i]
      puts "Your answer : " + $ans1[i]
      puts "The Correct answer : " + anst[i]
      count = count + 1 if $ans1[i] == anst[i]
    end
    puts "\nYour score: " + count.to_s + "/" + ques.length.to_s + "(" + format("%.2f",(count.to_f/ques.length.to_f)*100) + "%)"
  end
  return $ans1 if n == ques.length
  puts ques[n] , ansl[n]
  print "Your answer : "
  $ans1[n] = gets.chomp.upcase
  puts
  exam(n+1)
end
end


start = Question.new
start.exam(0)