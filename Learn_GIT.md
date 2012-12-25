* Learn GIT

** Version History

#Ver  Date       Author    Change                                                  
001   2012-12-25 kphuanghk Init the Learn_GIT.org.

** Local Version Control

New a local repository

git init

1. Edit a file.
2. git add <file>
3. git commit -m "<comment>"

** Remote Version Control

origin that is the default name Git gives to the server you cloned from:

git push origin master

git diff
git status

$ git commit -m 'initial commit'
$ git add forgotten_file
$ git commit --amend

git remote add gl https://github.com/HKGreenSolutions/greenlabs.git

** Using Tag


git config --global alias.visual '!gitk'

** Using Branch

$ git branch testing

Point existing HEAD to branch testing

$ git checkout testing

              Head
               |
               V
[C0] - [C1] - [C2]

Following two are equivalent:

Ver1: 
    $ git checkout -b iss53

Ver2:
    $ git branch iss53
    $ git checkout iss53
 
Merge the code together:
`$ git checkout master` --Put the pointer HEAD to master agian.
`$ git merge hotfix`



** Using Magit with Emacs

Download Magit, latest at this moment is 1.2.

Key commands
 - magit-status
 In Status Mode:
    "c" to commit
    "g/G" to refresh/refresh all
    "s" to stage the
 - magit-log
 - magit-diff
 - C-c C-c to confirm commit
 - C-c C-k to cancel commit

** Tips and Tricks

TBD.
