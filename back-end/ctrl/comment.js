var ObjectId = require('mongodb').ObjectID;

module.exports = (db) => {
  let comment = db.collection('comments');

  const resolve = Promise.resolve();

  function add (json) {
    comment.insert(json);
    return resolve;
  }

  function get (json) {
    console.log(json);
    return comment.find({'id': json.id})
           .toArray()
           .then(res => Promise.resolve(res))
           .catch(err => console.log(err));
  }

  function editNickname (json) {
    comment.update({'username': json.username}, {$set: {'nickname': json.nickname}}, {upsert: false, multi: true});
    return resolve;
  }


  return {
    add,
    get,
    editNickname
  }
};