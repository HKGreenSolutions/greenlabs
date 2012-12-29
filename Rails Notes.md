Use ActiveRecord Session
==========================================

    $rake db:sessions:create
    $rake db:migrate

Go to <Rapp>/config/initializers/session_store.rb

`Demo::Application.config.session_store :active_record_store`
